using System;
using System.Data;
using System.Xml.Linq;

namespace Kangal;

/// <summary>
/// Provides extension methods for the <see cref="IDataReader"/> interface
/// to enhance its functionality, such as converting its records to an XML format.
/// </summary>
public static partial class DataReaderExtensions
{
    /// <summary>
    /// Converts IDataReader records to XDocument.
    /// </summary>
    /// <param name="reader">IDataReader to read from</param>
    /// <param name="rootName">Root element name</param>
    /// <param name="nodeName">Node element name for each record</param>
    /// <returns>XDocument containing all records</returns>
    /// <exception cref="ArgumentNullException">Thrown when rootName or nodeName is null or empty</exception>
    /// <remarks>
    /// This method reads all records and materializes them into an XDocument.
    /// DBNull values are skipped and not included in the XML output.
    /// </remarks>
    public static XDocument ToXDocument(this IDataReader reader, string rootName, string nodeName)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootName);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);

        var xDocument = new XDocument(new XElement(rootName));

        while (reader.Read())
        {
            var xElement = new XElement(nodeName);

            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.IsDBNull(i))
                {
                    continue;
                }

                xElement.Add(new XElement(reader.GetName(i), reader[i]));
            }

            xDocument.Root?.Add(xElement);
        }

        return xDocument;
    }
}
