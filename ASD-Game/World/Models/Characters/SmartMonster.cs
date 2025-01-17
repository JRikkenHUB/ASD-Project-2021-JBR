﻿using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using ASD_Game.World.Models.Characters.Algorithms.NeuralNetworking;
using ASD_Game.World.Models.Characters.StateMachine.Data;
using ASD_Game.World.Services;

namespace ASD_Game.World.Models.Characters
{
    [ExcludeFromCodeCoverage]
    public class SmartMonster : Monster
    {
        public MonsterData CreatureData;
        public DataGatheringService _dataGatheringService { get; set; }
        public SmartCreatureActions Smartactions { get; set; }

        public Vector2 Destination { get; set; }
        public string MoveType { get; set; }

        public Genome Brain;
        public bool Replay = false;

        public static readonly int GenomeInputs = 14;
        public static readonly int GenomeOutputs = 8;

        public float[] Vision = new float[GenomeInputs];
        public float[] Decision = new float[GenomeOutputs];

        //Data for fitnessCalculation
        public int LifeSpan = 0;

        public bool Dead = false;
        public int DamageDealt { get; set; } = 0;
        public int DamageTaken { get; set; } = 0;
        public int HealthHealed { get; set; } = 0;
        public int StatsGained { get; set; } = 0;
        public int EnemysKilled { get; set; } = 0;

        public float CurrDistanceToPlayer;
        public float CurrDistanceToMonster;

        public SmartMonster(string name, int xPosition, int yPosition, string symbol, string id) : base(name, xPosition, yPosition, symbol, id)
        {
            CreatureData = CreateMonsterData(1);
            _dataGatheringService = null;
            Smartactions = null;
            Destination = new Vector2(xPosition, yPosition);
        }

        public void Update()
        {
            if (_dataGatheringService != null)
            {
                _dataGatheringService.CheckNewPosition(this);
                if (Health <= 0)
                {
                    Dead = true;
                }
                if (!Dead)
                {
                    LifeSpan++;
                    Look();
                    Think();
                }
            }
        }

        public void Look()
        {
            Vision[0] = CreatureData.Position.X;
            Vision[1] = CreatureData.Position.Y;
            Vision[2] = CreatureData.Damage;
            Vision[3] = (float)CreatureData.Health;
            _dataGatheringService.ScanMap(this, CreatureData.VisionRange);

            if (_dataGatheringService.ClosestPlayer == null)
            {
                Vision[4] = 9999;
                Vision[6] = 0;
                Vision[7] = 0;
                Vision[8] = 0;
                Vision[9] = 0;
            }
            else
            {
                Vision[4] = _dataGatheringService.DistanceToClosestPlayer;
                Vision[6] = (float)_dataGatheringService.ClosestPlayer.Health;
                Vision[7] = 10;//TODO _dataGatheringService.closestPlayer.Damage;
                Vision[8] = _dataGatheringService.ClosestPlayer.XPosition;
                Vision[9] = _dataGatheringService.ClosestPlayer.YPosition;
            }
            if (_dataGatheringService.ClosestMonster == null)
            {
                Vision[5] = 9999;
                Vision[10] = 0;
                Vision[11] = 0;
                Vision[12] = 0;
                Vision[13] = 0;
            }
            else
            {
                Vision[5] = _dataGatheringService.DistanceToClosestMonster;
                Vision[10] = (float)_dataGatheringService.ClosestMonster.Health;
                Vision[11] = 10;//TODO _dataGatheringService.closestMonster.Damage;
                Vision[12] = _dataGatheringService.ClosestMonster.XPosition;
                Vision[13] = _dataGatheringService.ClosestMonster.YPosition;
            }
        }

        public void Think()
        {
            float max = 0;
            int maxIndex = 0;
            //get the output of the neural network
            Decision = Brain.FeedForward(Vision);

            for (int i = 0; i < Decision.Length; i++)
            {
                if (Decision[i] > max)
                {
                    max = Decision[i];
                    maxIndex = i;
                }
            }

            if (max < 0.7)
            {
                Smartactions.Wander(this);
                return;
            }

            switch (maxIndex)
            {
                case 0:
                    Smartactions.Attack(_dataGatheringService.ClosestPlayer, this);
                    break;

                case 1:
                    Smartactions.Flee(_dataGatheringService.ClosestPlayer, this);
                    break;

                case 2:
                    Smartactions.RunToMonster(_dataGatheringService.ClosestMonster, this);
                    break;

                case 3:
                    Smartactions.RunToPlayer(_dataGatheringService.ClosestPlayer, this);
                    break;
            }
        }

        public void SetDifficulty(int difficulty)
        {
            MonsterData = new MonsterData(
                XPosition,
                YPosition,
                difficulty);
        }

        private MonsterData CreateMonsterData(int difficulty)
        {
            return new MonsterData(
                XPosition,
                YPosition,
                difficulty);
        }
    }
}