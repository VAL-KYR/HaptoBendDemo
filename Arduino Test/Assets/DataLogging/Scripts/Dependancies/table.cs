using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//+++ This will be the generic table class
public class Table
{
    //+++ Generic Row Column stores for 1 table
    public List<List<string>> row = new List<List<string>>();
}

//+++ This will be the generic data table class
public class DataTable
{
    //+++ Generic Row Column stores for 1 table
    public List<string> columnNames = new List<string>();
    public List<List<float>> row = new List<List<float>>();
}