using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiComponent : MonoBehaviour {
	
	public AiState initialState;
    protected AiState currentState;
    
	void Update(){
		if(currentState != null) {
			currentState.Tick(gameObject);
			if(currentState.IsBackTransitionRequested()) {
				if(currentState.previousState != null) {
					ActivateState(currentState.previousState);
				} else {
					Debug.LogWarning( "Requested previousState is NULL", this);
				}
			}else if(currentState.IsTransisionAllowed()) {
				ActivateState(currentState.nextState);
			}
		} else {
			ActivateState(initialState);
		}
	}

	private void ActivateState(AiState state) {
		currentState = state;
		state.OnEnter(gameObject);
	}
}