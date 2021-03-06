﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ConvNetCS
{
    [Serializable]
    public class Network
    {
        public List<ILayer> Layers { get; set; }
        public ILossLayer LossLayer { get; set; }
        public List<Vol> others = new List<Vol>();
         
        public event EventHandler ForwardLayer ;

        public Network()
        {
            Layers = new List<ILayer>();
        }
        public Vol Forward (Vol V, bool is_training)
        {

            var act = this.Layers[0].Forward(V, is_training);
            for (int i = 1; i < this.Layers.Count; i++)
            {
                 
                act = this.Layers[i].Forward(act, is_training);
                if (this.ForwardLayer!=null)
                {
                    this.ForwardLayer(this.Layers[i],new EventArgs());

                }
                 


            }
            act = this.LossLayer.Forward(act, is_training);
            return act;
        }


        public float Backward(int y)
        {
            var loss = this.LossLayer.Backward(y);

            for (int i = Layers.Count-1; i >= 0; i--)
            {
                Layers[i].Backward();
            }

            return loss;
        }

        public float Backward(float[] y)
        {
            var loss = (this.LossLayer as RegressionLayer).Backward(y);

            for (int i = Layers.Count - 1; i >= 0; i--)
            {
                Layers[i].Backward();
            }

            return loss;
        }
        
        public float Backward(ClassOutput y)
        {
            var loss = (this.LossLayer as RegressionLayer).Backward(y);

            for (int i = Layers.Count-1; i >= 0; i--)
            {
                Layers[i].Backward();
            }

            return loss;
        }
        
        public List<ParamsAndGrads> GetParamsAndGrads()
        {
            List<ParamsAndGrads> response = new List<ParamsAndGrads>();
            for (int i = 0; i < Layers.Count; i++)
            {
                response.AddRange(Layers[i].getParamsAndGrads());

            }
            response.AddRange(LossLayer.getParamsAndGrads());
            foreach (var item in others)
            {
                response.Add(new ParamsAndGrads() {  Grads=item.DW, Params=item.W, l1_decay_mul=0, l2_decay_mul=0});
            }
            return response;
        }

        public int GetPrediction()
        {
            var s = this.LossLayer;
            var p = s.Output.W;
            var maxv = p[0];
            var maxi = 0;
            for (int i = 1; i < p.Length; i++)
            {
                if (p[i]>maxv)
                {
                    maxv = p[i];
                    maxi = i;
                }
            }
            
            return maxi;
        }

        public Dictionary<int,float> GetTop5Prediction()
        {
            Dictionary<int, float> res = new Dictionary<int,float>(5);
            var s = this.LossLayer;
            var p = s.Output.W;  
            for (int j = 0; j < 5; j++)
            {
                 
               var maxi = 0;
                var maxv = 0.0f;
                for (int i = 0; i < p.Length; i++)
                {
                    if (p[i] > maxv && !res.Keys.Contains(i))
                    {
                        maxv = p[i];
                        maxi = i;
                    }
                }
                res.Add(maxi,maxv);
            }
            return res;
        }
        public int GetPrediction( float temp)
        {
            var s = this.LossLayer;
            var p = s.Output.W;
            var maxv = p[0];
            var maxi = 0;
            for (int i = 1; i < p.Length; i++)
            {
                var px = p[i] / temp;
                if (p[i] > maxv)
                {
                    maxv = px;
                    maxi = i;
                }
            }
            return maxi;
        }

        public void Save(string path)
        {
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryFormatter bf = new BinaryFormatter();
              bf.Serialize(fs,this) ;
        }

        public static Network Load(string path)
        {
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryFormatter bf = new BinaryFormatter();
            return  bf.Deserialize(fs) as Network;
        }
      
    }
}
