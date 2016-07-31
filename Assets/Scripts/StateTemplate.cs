using System;
using System.Collections;

public class StateTemplate<T> where T : IConvertible
{
	private T state;
	private T previewState;
	private T tiggerState;
	private float timeInState = 0.0f;
	private uint framesInState = 0;

	public StateTemplate( T initialState )
	{
		if (!typeof(T).IsEnum) 
		{
			throw new ArgumentException("T must be an enumerated type");
		}

		state = initialState;
		previewState = initialState;
		tiggerState = initialState;
		timeInState = 0.0f;
		framesInState = 0;
	}

	// Getter and setter for State
	public T value
	{
		get 
		{
			return state;
		}

		set
		{
			if( !state.Equals( value ) )
			{
				previewState = state;
				tiggerState = state;
				state = value;
				timeInState = 0.0f;
				framesInState = 0;
			}
		}
	}

	// Getter and setter for State
	public T previousValue
	{
		get 
		{
			return previewState;
		}
	}

	// Updates the timeInState value
	public void Update( float deltaTime )
	{
		timeInState += deltaTime;
		framesInState++;
	}

	// Returns the time in the current state
	public float TimeInState()
	{
		return timeInState;
	}

	// Returns the number of frames spend in the current state
	public uint FramesInState()
	{
		return framesInState;
	}

	// Returns true the frist time this funciton is called when state has changes
	public bool StateTriggered()
	{
		bool result = false;
		if( !tiggerState.Equals( state ) )
		{
			result = true;
			tiggerState = state;
		}

		return result;
	}
}


