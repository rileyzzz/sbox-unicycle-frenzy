/* ***************************************************************************
 * This file is part of SharpNEAT - Evolution of Neural Networks.
 * 
 * Copyright 2004-2016 Colin Green (sharpneat@gmail.com)
 *
 * SharpNEAT is free software; you can redistribute it and/or modify
 * it under the terms of The MIT License (MIT).
 *
 * You should have received a copy of the MIT License
 * along with SharpNEAT; if not, see https://opensource.org/licenses/MIT.
 */
using System.Threading.Tasks;

namespace SharpNeat.Core
{
    /// <summary>
    /// Generic interface for phenome evaluation classes.
    /// Evaluates and assigns a fitness to individual TPhenome's.
    /// </summary>
    public interface IPhenomeEvaluator<TPhenome>
    {
        /// <summary>
        /// Gets the total number of individual genome evaluations that have been performed by this evaluator.
        /// </summary>
        ulong EvaluationCount { get; }

        /// <summary>
        /// Gets a value indicating whether some goal fitness has been achieved and that
        /// the evolutionary algorithm search should stop. This property's value can remain false
        /// to allow the algorithm to run indefinitely.
        /// </summary>
        bool StopConditionSatisfied { get; }

		//public bool IsComplete(int index);

		//public void Run( int index, TPhenome box );

		/// <summary>
		/// Evaluate the provided phenome and return its fitness score.
		/// </summary>
		Task<FitnessInfo> Evaluate( int index, TPhenome phenome );

        /// <summary>
        /// Reset the internal state of the evaluation scheme if any exists.
        /// </summary>
        void Reset();
    }
}
