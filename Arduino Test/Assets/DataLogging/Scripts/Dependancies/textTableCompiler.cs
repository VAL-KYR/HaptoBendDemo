using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class textTableCompiler {

    //+++ MASS TEXT FORMATTING
    public static string FormatTable(DataTable table, bool rowCountHeader, bool rowColPrint, List<string> colNames, string cellSeparator, string lineSeparator)
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
        /// print row-col style
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
