using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Antymology.Terrain
{
    public class Queen_M : MonoBehaviour
    {
        #region fields
        public float Health;


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
            Health -= 0.1f;
            if (Health <= 0) enabled = false;
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