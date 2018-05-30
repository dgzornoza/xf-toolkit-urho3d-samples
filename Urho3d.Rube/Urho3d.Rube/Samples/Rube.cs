﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Urho;

namespace Urho3d.Rube.Samples
{
    public class Rube
    {
        public Rube() { }

        public void LoadWorld(Scene scene)
        {
            string filePath = Urho.Application.Current.ResourceCache.GetResourceFileName("Urho2D/RubePhysics/documentA.json");
            Toolkit.Urho.Rube.B2dJson b2dJson = new Toolkit.Urho.Rube.B2dJson();
            b2dJson.ReadIntoSceneFromFile(filePath, scene, out string errorMsg);            
        }
    }
}
