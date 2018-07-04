using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class textTableCompiler {

    //// TABLE OPERATIONS ////
    //+++ CREATE MASS DATA TABLE
    /*
    public static Table CombineTables(params Table[] subTables)
    {
        /// data compiled into a generic table
        Table massTable = new Table();
        int finalRows = 0;
        int finalCols = 0;

        // find out how large the final table will be
        foreach (Table sub in subTables)
        {
            finalRows += sub.totalRows;
            finalCols += sub.totalCols;
        }

        // set the massTable final table to have the appropriate size
        massTable.totalRows = finalRows;
        massTable.totalCols = finalCols;

        /// FILL TABLE ALL
        int currentRows = 0;
        int currentCols = 0;

        foreach (Table sub in subTables)
        {
            // rows
            for (int x = currentRows; x < massTable.totalRows; x++)
            {
                // columns compile
                for (int y = currentCols; y < massTable.totalCols; y++)
                {
                    if (massTable.col.Count < massTable.totalCols)
                    {
                        // massTable.col.Add(sub.col[y]);
                        // this is just a test which replaces all cells with "test"
                        // eventually real data will be substituted dynamically by the size of each new subtable in sequence
                        massTable.col.Add("test");
                    }
                    else
                    {
                        // massTable.col[y] = sub.col[y];
                        // this is just a test which replaces all cells with "test"
                        // eventually real data will be substituted dynamically by the size of each new subtable in sequence
                        massTable.col[y] = "test";
                    }
                }

                // rows compile
                if (massTable.row.Count < massTable.totalRows)
                {
                    massTable.row.Add(massTable.col);
                }
                else
                {
                    massTable.row[x] = massTable.col;
                }
            }

            // Running starting point for a table's appending
            currentRows += sub.totalRows;
            currentCols += sub.totalCols;

        }

        return massTable;

    }

    //+++ MASS DATA TO TEXT COVERSION
    // this is borken and can't initialize a table properly?
    public static Table TableDataToTextTable(DataTable data)
    {
        Table formatted = new Table();

        formatted.row = new List<List<string>>(data.row.Count);

        foreach (List<float> row in data.row)
        {
            formatted.row.Add(new List<string>(data.row[0].Count));
        }

        for (int rows = 0; rows < data.row.Count; rows++)
        {
            formatted.row.Add(new List<string>(data.row[rows].Count));

            for (int cell = 0; cell < data.row[rows].Count; cell++)
            {
                formatted.row[rows][cell] = data.row[rows][cell].ToString();
            }
        }

        return formatted;
    }
    
    */



    //+++ MASS TEXT FORMATTING
    public static string FormatTable(DataTable table, bool rowCountHeader, List<string> colNames, string cellSeparator, string lineSeparator)
    {
        // return string
        string formattedLine = "";
        int rowCount = 0;

        // Start new data on a new line always
        formattedLine += lineSeparator;

        // if we're using column names add them
        if (colNames != null)
        {
            // Column Names in header
            foreach (string header in colNames)
            {
                formattedLine += header + cellSeparator;
            }

            // New Line before data
            formattedLine += lineSeparator;
        }

        /// data sent to the export string
        foreach (List<float> row in table.row)
        {
            if (rowCountHeader)
            {
                formattedLine += rowCount + cellSeparator;
            }

            foreach (float cell in row)
            {
                formattedLine += cell + cellSeparator;
            }

            formattedLine += lineSeparator;

            rowCount++;
        }

        return formattedLine;
    }
}
