using System;
using Sandbox;

public struct Neuron
{
	public double Output;
	public double[] Weights { get; set; }

	public Neuron Clone()
	{
		Neuron neuron = new Neuron();
		neuron.Output = Output;
		if (Weights != null)
			neuron.Weights = (double[])Weights.Clone();
		return neuron;
	}
}

public class NeuronLayer
{
	public Neuron[] Neurons { get; set; }
	public float Bias { get; set; } = 1.0f;

	protected NeuronLayer()
	{
	}

	public NeuronLayer(int numNeurons)
	{
		Neurons = new Neuron[numNeurons];
	}

	public NeuronLayer Clone()
	{
		NeuronLayer layer = new NeuronLayer();
		layer.Neurons = new Neuron[Neurons.Length];
		for ( int i = 0; i < Neurons.Length; i++ )
			layer.Neurons[i] = Neurons[i].Clone();
		layer.Bias = Bias;
		return layer;
	}
}

public class UnicycleBrain
{
	public NeuronLayer[] Layers { get; set; }
	NeuronLayer Inputs => Layers[0];
	NeuronLayer Outputs => Layers[Layers.Length - 1];

	protected UnicycleBrain()
	{
	}

	public UnicycleBrain(int inputs, int outputs, int layers, int layerSize)
	{
		Layers = new NeuronLayer[layers + 2];
		Layers[0] = new NeuronLayer( inputs );
		Layers[Layers.Length - 1] = new NeuronLayer( outputs );
		for ( int i = 1; i < Layers.Length - 1; i++ )
			//Layers[i] = new NeuronLayer(2 * inputs + 1);
			Layers[i] = new NeuronLayer( layerSize );

		// initialize all weights
		for ( int i = 1; i < Layers.Length; i++ )
		{
			NeuronLayer last = Layers[i - 1];

			// for each node of this layer
			for ( int j = 0; j < Layers[i].Neurons.Length; j++ )
			{
				Layers[i].Neurons[j].Weights = new double[last.Neurons.Length];
				for ( int w = 0; w < last.Neurons.Length; w++ )
					//Layers[i].Neurons[j].Weights[w] = 1.0f;
					//Layers[i].Neurons[j].Weights[w] = Rand.Float();
					Layers[i].Neurons[j].Weights[w] = Rand.Float(-1.0f, 1.0f);
			}
		}

		//Layers = new float[layers, 2 * inputs / 3 + outputs];
		//Layers = new float[layers, 2 * inputs + 1];
	}

	public UnicycleBrain Clone()
	{
		UnicycleBrain brain = new UnicycleBrain();

		brain.Layers = new NeuronLayer[Layers.Length];
		for ( int i = 0; i < brain.Layers.Length; i++ )
			brain.Layers[i] = Layers[i].Clone();

		return brain;
	}

	public static UnicycleBrain Cross(UnicycleBrain a, UnicycleBrain b)
	{
		UnicycleBrain child = new UnicycleBrain();

		child.Layers = new NeuronLayer[a.Layers.Length];
		for ( int i = 0; i < child.Layers.Length; i++ )
		{
			NeuronLayer a_layer = a.Layers[i];
			NeuronLayer b_layer = b.Layers[i];
			NeuronLayer child_layer = new NeuronLayer(a_layer.Neurons.Length);
			child_layer.Bias = a_layer.Bias;
			child.Layers[i] = child_layer;
			for (int j = 0; j < child_layer.Neurons.Length; j++)
			{
				Neuron a_n = a_layer.Neurons[j];
				Neuron b_n = b_layer.Neurons[j];
				ref Neuron n = ref child_layer.Neurons[j];
				if ( a_n.Weights == null || b_n.Weights == null )
					continue;

				n.Weights = new double[a_n.Weights.Length];
				for ( int k = 0; k < n.Weights.Length; k++ )
					n.Weights[k] = Rand.Float() > 0.5f ? a_n.Weights[k] : b_n.Weights[k];
			}
		}

		return child;
	}

	public void Mutate( float mutationScale, float mutationChance )
	{
		for ( int i = 1; i < Layers.Length; i++ )
		{
			NeuronLayer layer = Layers[i];
			for ( int j = 0; j < layer.Neurons.Length; j++ )
			{
				ref Neuron n = ref layer.Neurons[j];
				for ( int k = 0; k < n.Weights.Length; k++ )
				{
					if ( Rand.Float() < mutationChance )
						n.Weights[k] += Rand.Float( -1.0f, 1.0f ) * mutationScale;
				}
			}
		}
	}

	public void SetInputs( double[] inputs )
	{
		for ( int i = 0; i < inputs.Length; i++ )
			Inputs.Neurons[i].Output = inputs[i];
	}

	public double[] GetOutputs()
	{
		double[] outputs = new double[Outputs.Neurons.Length];
		for ( int i = 0; i < Outputs.Neurons.Length; i++ )
			outputs[i] = Outputs.Neurons[i].Output;
		return outputs;
	}

	public void Execute()
	{
		// start at first hidden layer
		for ( int i = 1; i < Layers.Length; i++ )
		{
			NeuronLayer last = Layers[i - 1];

			// for each node of this layer
			for ( int j = 0; j < Layers[i].Neurons.Length; j++ )
			{
				ref Neuron n = ref Layers[i].Neurons[j];

				double sum = 0.0f;
				// for each node of the last layer
				for (int k = 0; k < last.Neurons.Length; k++ )
					sum += last.Neurons[k].Output * n.Weights[k];

				double sig = 1.0f / (1.0f + Math.Exp(-sum));
				n.Output = sig;
			}
		}
	}
}
