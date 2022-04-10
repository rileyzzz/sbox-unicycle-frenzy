﻿/* ***************************************************************************
 * This file is part of the Redzen code library.
 * 
 * Copyright 2015-2019 Colin Green (colin.green1@gmail.com)
 *
 * Redzen is free software; you can redistribute it and/or modify
 * it under the terms of The MIT License (MIT).
 *
 * You should have received a copy of the MIT License
 * along with Redzen; if not, see https://opensource.org/licenses/MIT.
 */
using Sandbox;
using System;
using System.Security.Cryptography;
using System.Threading;

namespace Redzen.Random
{
    /// <summary>
    /// Default implementation of IRandomSeedSource.
    /// A source of seed values for use by pseudo-random number generators (PRNGs).
    /// </summary>
    /// <remarks>
    /// This implementation uses multiple seed PRNGs initialised with high quality crypto random seed state. 
    /// 
    /// New seeds are generated by rotating through the seed PRNGs to generate seed values. Using multiple seed PRNGs
    /// in this way (A) increases the state space that is being sampled from, (B) improves thread concurrency by 
    /// allowing each PRNG to be in use concurrently, and (C) greatly improves performance compared to using
    /// a crypto random source for all PRNGs. I.e. this class is a compromise between using perfectly random 
    /// (crypto) seed data, versus using pseudo-random data but with increased performance.
    /// </remarks>
    public class DefaultRandomSeedSource : IRandomSeedSource
    {
        #region Instance Fields

        readonly uint _concurrencyLevel;
		readonly Xoshiro256StarStarRandom[] _seedRngArr;
		//readonly SandboxRandom[] _seedRngArr;
        readonly object[] _lockArr;
        // Round robin accumulator.
        int _roundRobinAcc = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct with the default concurrency level.
        /// </summary>
        public DefaultRandomSeedSource()
            //: this(Environment.ProcessorCount)
            : this(1)
        {}

        /// <summary>
        /// Construct with the specified minimum concurrency level.
        /// </summary>
        /// <remarks>
        /// minConcurrencyLevel must be at least one, an exception is thrown if it is less than 1 (i.e. zero or negative).
        /// The actual concurrency level is required to be a power of two, thus the actual level is chosen to be the 
        /// nearest power of two that is greater than or equal to minConcurrencyLevel/
        /// </remarks>
        public DefaultRandomSeedSource(int minConcurrencyLevel)
        {
            if(minConcurrencyLevel < 1) {
                throw new ArgumentException("Must be at least 1.", nameof(minConcurrencyLevel));
            }

            // The actual concurrency level is required to be a power of two, thus the actual level is chosen
            // to be the nearest power of two that is greater than or equal to minConcurrencyLevel.
            int concurrencyLevel = MathUtils.CeilingToPowerOfTwo(minConcurrencyLevel);
            _concurrencyLevel = (uint)concurrencyLevel;

            // Create high quality random bytes to init the seed PRNGs.
            byte[] buf = GetCryptoRandomBytes(concurrencyLevel * 8);

            // Init the seed PRNGs and associated sync lock objects.
            // Note. In principle we could just use each RNG object as the sync lock for itself, but that is considered bad practice.
            _seedRngArr = new Xoshiro256StarStarRandom[concurrencyLevel];
            _lockArr = new object[concurrencyLevel];

            for(int i=0; i < concurrencyLevel; i++)
            {
                // Init rng.
                ulong seed = BitConverter.ToUInt64(buf, i * 8);
                _seedRngArr[i] = new Xoshiro256StarStarRandom( seed );
                _lockArr[i] = new object();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get a new seed value.
        /// </summary>
        public ulong GetSeed()
        {
            // Select seed RNGs by cycling through them.
            //
            // _concurrencyLevel is required to be a power of two, so that the modulus result cycles 
            // through the seed RNG indexes without jumping when _roundRobinAcc transitions from 
            // 0xffff_ffff to 0x0000_0000.
            // Note. The modulus operation is generally expensive to compute; here a much cheaper/faster
            // alternative method can be used because _concurrencyLevel is guaranteed to be a power
            // of two.
            uint idx = ((uint)Interlocked.Increment(ref _roundRobinAcc)) & (_concurrencyLevel-1);

            // Get a lock on the chosen PRNG.
            // If GetSeed() is called with very high frequency then the round robin cycling could arrive
            // back at an entry before its lock has been released, in which case we just have to wait for
            // that lock to be released. If this is observed to be occurring then a higher concurrency level
            // should be used.
            lock(_lockArr[idx])
            {
                // Obtain a random sample from the selected seed RNG and release the lock.
                return _seedRngArr[idx].NextULong();
            }
        }

        #endregion

        #region Private Static Methods

        private static byte[] GetCryptoRandomBytes(int count)
        {
			System.Random r = new System.Random();
			byte[] buf = new byte[count];
			r.NextBytes(buf);
			return buf;

            //// Note. Generating crypto random bytes can be very slow, relative to a PRNG; we may even have to wait
            //// for the OS to have sufficient entropy for generating the bytes.
            //byte[] buf = new byte[count];
            //using(RNGCryptoServiceProvider cryptoRng = new RNGCryptoServiceProvider())
            //{
            //    cryptoRng.GetBytes(buf);
            //}
            //return buf;
        }

        #endregion
    }
}
