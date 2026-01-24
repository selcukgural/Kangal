using System;
using System.Data;
using System.IO;
using System.Xml.Linq;

namespace Kangal;

/// <summary>
/// Provides extension methods for the <see cref="DataTable"/> class to enhance its functionality.
/// </summary>
public static partial class DataTableExtensions
{
    /// <summary>
    /// Converts a DataTable to an XDocument representation.
    /// </summary>
    /// <param name="dataTable">The DataTable to be converted.</param>
    /// <param name="xmlWriteMode">Specifies the XML write mode for the DataTable serialization.</param>
    /// <param name="nodeName">Optional custom name for the XML node representing the DataTable. Defaults to the DataTable's existing name or "nodes" if no name is provided.</param>
    /// <param name="writeHierarchy">Indicates whether hierarchical relationships within the DataTable should be included in the XML structure.</param>
    /// <returns>An XDocument containing the serialized content of the DataTable.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided DataTable is null.</exception>
    public static XDocument ToXDocument(this DataTable dataTable, XmlWriteMode xmlWriteMode = XmlWriteMode.IgnoreSchema, string? nodeName = null,
                                        bool writeHierarchy = true)
    {
        ArgumentNullException.ThrowIfNull(dataTable);

        var originalTableName = dataTable.TableName;

        try
        {
            dataTable.TableName = string.IsNullOrEmpty(nodeName)
                                      ? string.IsNullOrEmpty(originalTableName) ? "nodes" : originalTableName 
                                      : nodeName;
            
            using var stringWriter = new StringWriter();
            dataTable.WriteXml(stringWriter, xmlWriteMode, writeHierarchy);
            return XDocument.Parse(stringWriter.ToString());
        }
        finally
        {
            dataTable.TableName = originalTableName;
        }
    }
}