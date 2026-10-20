using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public class TransferomPoseProxy : IPoseProxy
    {
        Transform transform;

        public TransferomPoseProxy (Transform transform)
        {
            this.transform = transform;
        }

        Vector3 IPoseProxy.Pos => transform.position;

        Vector3 IPoseProxy.Euler => transform.eulerAngles;
    }

    public class GeneralPoseProxy : IPoseProxy
    {
        Func<Vector3> posGetter;
        Func<Vector3> eulerGetter;

        public GeneralPoseProxy (Func<Vector3> posGetter, Func<Vector3> eulerGetter)
        {
            this.posGetter = posGetter;
            this.eulerGetter = eulerGetter;
        }

        Vector3 IPoseProxy.Pos => posGetter.Invoke ();

        Vector3 IPoseProxy.Euler => eulerGetter.Invoke ();
    }

    public interface IPoseProxy
    {
        Vector3 Pos { get; }
        Vector3 Euler { get; }
    }
}

