using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class textTableCompiler {

    //// TABLE OPERATIONS ////
    //+++ CREATE MASS DATA TABLE
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


    //+++ MASS TEXT FORMATTING
    public static string FormatTable(Table table, string cellSeparator, string lineSeparator)
    {
        // return string
        string formattedLine = "";

        /// data sent to the export string
        foreach (List<string> row in table.row)
        {
            foreach (string col in row)
            {
                formattedLine += col + cellSeparator;
            }

            formattedLine += lineSeparator;
        }

        return formattedLine;
    }    
}
