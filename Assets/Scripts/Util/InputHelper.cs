using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 추 후 변경
public class InputHelper
{
    Hashtable mButtonTimeout;

    public InputHelper() { mButtonTimeout = new Hashtable(); }

    public bool OnKeyPressed(KeyCode key)
    {
        if (Input.GetKey(key))
        {
            if (!mButtonTimeout.ContainsKey(key))
            {
                mButtonTimeout.Add(key, true);
                return true;
            }
        }
        else
            if (mButtonTimeout.ContainsKey(key))
                mButtonTimeout.Remove(key);
        return false;
    }
}
