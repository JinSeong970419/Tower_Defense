using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class OddsTable
{
    float mOddsTotal;
    SortedDictionary<string, Vector2> mOdds;
    Hashtable mOddPayload;

    public OddsTable()
    {
        mOdds = new SortedDictionary<string, Vector2>();
        mOddPayload = new Hashtable();
        mOddsTotal = 0;
    }

    public void Add(string key, float weight, object payload = null)
    {
        mOdds.Add(key, new Vector2(mOddsTotal, mOddsTotal += weight));
        mOddPayload.Add(key, payload);
    }

    public string Roll() { return getResultKey(Random.Range(0.0f * 1000F, mOddsTotal * 1000F) / 1000F); }
    public string GetForNormalizedRoll(float dec)
    {
        //Debug.Log(dec * mOddsTotal);
        return getResultKey(dec * mOddsTotal);
    }

    protected string getResultKey(float randomNumber)
    {
        string result = "None";
        foreach (KeyValuePair<string, Vector2> entry in mOdds)
        {
            if (entry.Value.x <= randomNumber && randomNumber <= entry.Value.y)
            {
                result = entry.Key;
                break;
            }
        }
        return result;
    }

    // key 개체 검색
    public object GetPayload(string key)
    {
        if (mOddPayload.ContainsKey(key))
            return mOddPayload[key];
        return null;
    }
}
