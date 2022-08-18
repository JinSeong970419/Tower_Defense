using System;
using UnityEngine;

public class Status : MonoBehaviour
{
    public virtual void Copy(Status status){
        throw new Exception("덮어쓰기");
    }

    public virtual void Apply(GameObject target, GameObject projectile) {
        throw new Exception("덮어쓰기");
    }

    public virtual void Refresh() {
        throw new Exception("덮어쓰기");
    }
}
