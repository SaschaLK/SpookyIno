﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	private static GameManager instance;
	public static GameManager Instance {
		get{
			return instance;
		}
	}

	void Awake(){
		if(instance != null){
			Debug.LogError("GameManager instance is already present");
			Destroy(gameObject);
			return;
		}

		instance = this;
	}

    public bool InDungeon = false;

	public InventoryItemList itemDatabase;
	public ChunkTypeDataList chunkOutsideDatabase;
	public ChunkTypeDataList chunkDungeonDatabase;
    public ObjectiveList objectiveDatabase;
    public SoundList audioDatabase;

}
