using Assets.Scripts.DataStructures;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Locomotion))]
    public class CharacterBehaviour : MonoBehaviour
    {

        protected Locomotion LocomotionController;
        protected AbstractPathMind PathController;
        public BoardManager BoardManager { get; set; }
        protected CellInfo currentTarget;

        //Online Variables
        public bool online;
        //Tiempo de decision
        public int tiempoDePensamiento = 3;
        private int currentTiempoPensamiento = 0;
        private List<GameObject> enemies = new List<GameObject>();

        void Start()
        {

            PathController = GetComponentInChildren<AbstractPathMind>();
            PathController.SetCharacter(this);
            LocomotionController = GetComponent<Locomotion>();
            LocomotionController.SetCharacter(this);
            var aux = GameObject.FindGameObjectsWithTag("Enemy");
            enemies = new List<GameObject>(aux);


        }

        void Update()
        {

            if (BoardManager == null) return;

            if (online)
            {
                //Cambiamos el objetivo
                if (this.currentTarget == null || !this.currentTarget.Walkable)
                {
                    changeTarget();
                    PathController.Repath();
                    var boardClone = (BoardInfo)BoardManager.boardInfo.Clone();
                    LocomotionController.SetNewDirection(PathController.GetNextMove(boardClone, LocomotionController.CurrentEndPosition(), new[] { this.currentTarget }));

                }

                if (LocomotionController.MoveNeed)
                {
                    currentTiempoPensamiento++;

                    //Volvemos a buscar
                    if (currentTiempoPensamiento >= tiempoDePensamiento)
                    {
                        PathController.Repath();
                        var boardClone = (BoardInfo)BoardManager.boardInfo.Clone();
                        LocomotionController.SetNewDirection(PathController.GetNextMove(boardClone, LocomotionController.CurrentEndPosition(), new[] { this.currentTarget }));
                        currentTiempoPensamiento = 0;

                    }
                    //Ejucutamos el pan actual
                    else
                    {
                        var boardClone = (BoardInfo)BoardManager.boardInfo.Clone();
                        LocomotionController.SetNewDirection(PathController.GetNextMove(boardClone, LocomotionController.CurrentEndPosition(), new[] { this.currentTarget }));
                                                                       
                    }

                    if (this.currentTarget == LocomotionController.CurrentEndPosition())
                    {
                        SetCurrentTarget(null);
                        currentTiempoPensamiento = tiempoDePensamiento;
                    }


                }
            }
            else
            {
                if (LocomotionController.MoveNeed)
                {

                    var boardClone = (BoardInfo)BoardManager.boardInfo.Clone();
                    LocomotionController.SetNewDirection(PathController.GetNextMove(boardClone, LocomotionController.CurrentEndPosition(), new[] { this.currentTarget }));
                    return;
                }
            }
        }



        public void SetCurrentTarget(CellInfo newTargetCell)
        {
            this.currentTarget = newTargetCell;
        }

        public void changeTarget()
        {
            if (enemies.Count > 0 && enemies[0] == null ) {
                enemies.RemoveAt(0);
            }

            if (enemies.Count > 0)
            {
                
                this.currentTarget = enemies[0].GetComponent<EnemyBehaviour>().CurrentPosition();
            }
            else
            {
                this.currentTarget = BoardManager.boardInfo.Exit;
            }
        }

        public bool esMeta(CellInfo currentPos)
        {
            if (currentPos.GetPosition == this.currentTarget.GetPosition)
                return true;

            return false;
        }
    }
}

