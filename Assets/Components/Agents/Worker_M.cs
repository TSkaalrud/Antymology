using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Antymology.Terrain
{



    public class Worker_M : MonoBehaviour
    {

        #region fields
        public float Health;
        protected float HealthThreshold = 100; //gene opportunity
        bool legalDigs = false;
        public bool isQueen;
        /// <summary>
        /// 0 = x,  z   the ant's air block
        /// 1 = x,  z+1 adjacent 1
        /// 2 = x+1,z   adjacent 2
        /// 3 = x,  z-1 adjacent 3
        /// 4 = x-1,z   adjacent 4
        /// 5 = y-1     the ground beneath the ant
        /// </summary>
        public AbstractBlock[] Adjacents = new AbstractBlock[6];
        
        //list of adjacents options for moving
        public List<int> legalMoves = new List<int>();


        //last position to move back if on poison or whatever?

        protected GameObject world;
        protected WorldManager manager;



        #endregion
        //public GameObject Instance;


        // Start is called before the first frame update
        void Start()
        {
            Health = 100;
            world = GameObject.Find("WorldManager");
            manager = world.GetComponent<WorldManager>();
        }

        // Update is called once per frame
        void Update()
        {
            starve();
            brain();
            if (isQueen)
            {
                buildNest();
            }
        }

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }

        #region helpers

        protected void brain()
        {
            updateAdjacents();
            //AirBlock a = (AirBlock) manager.GetBlock(69, 25, 39);
            //print(a.phermoneDeposits);
            //heightDifference(this.transform.position.x, this.transform.position.z + 1);
            //digging
            //only dig if you can get out*
            //only eat if you're above the eating threshold (so as not to waste food)
            if (legalToDig())
            {
                //dig out grass, acid, and stone; dig out mulch if below healthThreshold
                string type = Adjacents[5].GetType().Name;
                if (type == "AcidicBlock" || type == "GrassBlock" || type == "StoneBlock")
                {
                    DigBlock();
                }
                else if (type == "MulchBlock" && Health <= HealthThreshold)
                {
                    eat();
                }
            }

            //print(heightDifference(this.transform.position.x, this.transform.position.z));
            //print(heightDifference(this.transform.position.x, this.transform.position.z + 1));
            //print(heightDifference(this.transform.position.x + 1, this.transform.position.z));
            //print(heightDifference(this.transform.position.x, this.transform.position.z - 1));
            //print(heightDifference(this.transform.position.x - 1, this.transform.position.z));

            //moving
            //randomly unless there are pheromones to direct you
            //if pheromones are present, follow the strongest ones according to their rules
            updateLegalMoves();
            if (legalMoves.Count > 0)
            {
                //print(legalMoves.Count);

                int targetMove = legalMoves[manager.RNG.Next(0, legalMoves.Count)];
                //print(targetMove);
                //print(Adjacents[targetMove].worldXCoordinate + " " + Adjacents[targetMove].worldYCoordinate + " " + Adjacents[targetMove].worldZCoordinate);
                move(Adjacents[targetMove].worldXCoordinate, Adjacents[targetMove].worldYCoordinate, Adjacents[targetMove].worldZCoordinate);
            }

            //ensure the ant is still in play?
            if (this.transform.position.x == 88 && this.transform.position.y == 6.5 && this.transform.position.z == 101)
            {
                this.transform.position = manager.getAirBlockLoc(manager.RNG.Next(1, manager.Blocks.GetLength(0) - 1), manager.RNG.Next(1, manager.Blocks.GetLength(2) - 1));
            }
        }

        /// <summary>
        /// Trigger the ant to lose health and potentially die
        /// </summary>
        protected void starve()
        {
            Health -= 0.1f;
            if (Health <= 0) enabled = false;
        }

        protected void eat()
        {
            this.Health = 100;
            DigBlock();
        }

        /// <summary>
        /// Returns the block at the given x/y/z 
        /// (Type can then be fetched via e.g. block.GetType().Name == "AirBlock")
        /// *all blocks are on a 0.5, so y+1 is factored here
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        protected AbstractBlock GetBlock(float x, float y, float z)
        {
            AbstractBlock block = manager.GetBlock((int) x, (int) y+1, (int) z);
            //print(manager.GetBlock((int)x+1, (int)y + 1, (int)z).worldXCoordinate);
            return block;
            //world.GetComponent<WorldManager>().GetBlock((int)transform.position.x, (int)transform.position.y - 1, (int)transform.position.z) as ContainerBlock != null
        }

        /// <summary>
        /// "Digs" out the block directly below the ant replacing it with air and moving the ant down with it.
        /// *all blocks are on a 0.5, so y+1 is factored here
        /// </summary>
        /// <param xCoord="x"></param>
        /// <param yCoord="y"></param>
        /// <param zCoord="z"></param>
        protected void DigBlock()
        {
            AirBlock air = new AirBlock();
            air.worldXCoordinate = (int)this.transform.position.x;
            air.worldYCoordinate = (int)this.transform.position.y - 1 + 1;
            air.worldZCoordinate = (int)this.transform.position.z;

            manager.SetBlock((int)this.transform.position.x, (int)this.transform.position.y-1+1, (int)this.transform.position.z, air);
            //move(this.transform.position.x, (this.transform.position.y-1), this.transform.position.z);
        }

        /// <summary>
        /// returns the height difference between an ant's current position and the surface (lowest) air block of an input x/z.
        /// failing to find an air block, the height difference returns a number too large to be considered a legal move.
        /// </summary>
        /// <param XCoord="x"></param>
        /// <param ZCoord="z"></param>
        /// <returns></returns>
        protected float heightDifference(float x, float z)
        {
            AbstractBlock surfaceAir = manager.getAirBlock((int)x, (int)z);
            if (surfaceAir != null) return (this.transform.position.y - surfaceAir.worldYCoordinate + 0.5f);
            //print((int)(this.transform.position.y - surfaceAir.worldYCoordinate + 1));
            return 1000;
        }

        /// <summary>
        /// Get the ant's 4 adjacent options for decision making
        /// </summary>
        protected void updateAdjacents()
        {
            Adjacents[0] = GetBlock(this.transform.position.x, this.transform.position.y, this.transform.position.z);
            Adjacents[5] = GetBlock(this.transform.position.x, this.transform.position.y-1, this.transform.position.z);

            Adjacents[1] = GetBlock(this.transform.position.x, (this.transform.position.y - heightDifference(this.transform.position.x, this.transform.position.z+1)), this.transform.position.z+1);
            Adjacents[2] = GetBlock(this.transform.position.x+1, (this.transform.position.y - heightDifference(this.transform.position.x+1, this.transform.position.z)), this.transform.position.z);
            Adjacents[3] = GetBlock(this.transform.position.x, (this.transform.position.y - heightDifference(this.transform.position.x, this.transform.position.z-1)), this.transform.position.z-1);
            Adjacents[4] = GetBlock(this.transform.position.x-1, (this.transform.position.y - heightDifference(this.transform.position.x-1, this.transform.position.z)), this.transform.position.z);

            //for (int i = 0; i < 6; i++)
            //{
            //    print(Adjacents[i].GetType().Name + " " + Adjacents[i].worldXCoordinate + " " + Adjacents[i].worldYCoordinate + " " + Adjacents[i].worldZCoordinate);

            //}
            //print(Adjacents[1].GetType().Name);
        }

        /// <summary>
        /// updates the refreshing list of legal block options to choose from
        /// </summary>
        protected void updateLegalMoves()
        {
            legalMoves.Clear();

            for (int i = 1; i < 5; i++)
            {
                if (Mathf.Abs(heightDifference(Adjacents[i].worldXCoordinate, Adjacents[i].worldZCoordinate)) <= 2) legalMoves.Add(i);
                //print(Mathf.Abs(heightDifference(Adjacents[i].worldXCoordinate, Adjacents[i].worldZCoordinate)));
            }
        }

        /// <summary>
        /// updates the refreshing list of legal block options to choose from
        /// </summary>
        protected bool legalToDig()
        {
            legalDigs = false;
            //for each direction check to see if you'll be able to move after digging
            for (int i = 0; i < legalMoves.Count; i++)
            {
                if (heightDifference(Adjacents[legalMoves[i]].worldXCoordinate, Adjacents[legalMoves[i]].worldZCoordinate) <= 1 &&
                    heightDifference(Adjacents[legalMoves[i]].worldXCoordinate, Adjacents[legalMoves[i]].worldZCoordinate) >= -1)
                {
                    print(heightDifference(Adjacents[legalMoves[i]].worldXCoordinate, Adjacents[legalMoves[i]].worldZCoordinate));
                    legalDigs = true;
                    break;
                }
            }
            return legalDigs;
        }

        /// <summary>
        /// Make the ant look in the direction it is going to move and then move there
        /// Doesn't check for validity of movement*
        /// </summary>
        /// <param xCoord="x"></param>
        /// <param yCoord="y"></param>
        /// <param zCoord="z"></param>
        protected void move(float x, float y, float z)
        {
            //look in the direction you were moving
            Vector3 targetPos = new Vector3(x, y - 0.5f, z);
            Vector3 direction = targetPos - this.transform.position;
            this.transform.rotation = Quaternion.LookRotation(direction,new Vector3(0,1,0));

            //move
            this.transform.position = targetPos;
        }


        private void buildNest()
        {
            if (Health > 50)
            {
                NestBlock nest = new NestBlock();
                nest.worldXCoordinate = (int)this.transform.position.x;
                nest.worldYCoordinate = (int)this.transform.position.y + 1;
                nest.worldZCoordinate = (int)this.transform.position.z;

                manager.SetBlock((int)this.transform.position.x, (int)this.transform.position.y + 1, (int)this.transform.position.z, nest);
                Health -= 33;
                manager.nestBlocks += 1;
            }
        }
        #endregion

    }
}