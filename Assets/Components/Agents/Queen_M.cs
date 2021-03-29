using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Antymology.Terrain
{
    public class Queen_M : Worker_M
    {
        #region fields
        //public float Health;



        #endregion
        //public GameObject Instance;




        // Start is called before the first frame update
        void Start()
        {
            Health = 100;
        }

        // Update is called once per frame
        void Update()
        {
            starve();
            brain();
            buildNest();
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {
            //end game if the queen dies
        }

        #region helpers

        void buildNest()
        {

        }

        #endregion

    }
}