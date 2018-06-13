using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//+++ This will be the generic table class
public class Table
{
    //+++ Generic Row Column stores for 1 table
    public int totalRows = 5;
    public int totalCols = 5;
    public List<List<string>> row = new List<List<string>>();
    public List<string> col = new List<string>();
}
