using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Neo4j.Driver.V1;
using SeibelCases.Seibel;

namespace SeibelCases
{
    public class GraphQuery : IDisposable
    {
        private readonly IDriver _driver;
        public GraphQuery(string uri, string user, string password)
        {
            if (_driver == null)
            {
                _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
            }
        }
        public void Dispose() => _driver?.Dispose();
        public void RunQuery(string query)
        {
            using (var session = _driver.Session())
            {
                session.Run(query);
            }
        }
        public static string CreateAreaNode(string area)
        {
            return "MERGE (:Area {name: \"" + area + "\"})";
        }
        public static string CreateAreaNode(string area, string nodeVariable)
        {
            return "MERGE (" + nodeVariable + ":Area {name: \"" + area + "\"})";
        }
        public static string CreateCaseNode(string caseNo, string summary, string tags)
        {
            return "MERGE (:SeibelCase {sr: \"" + caseNo + "\", summary: \"" + summary + "\", tags: [" + tags + "]})";
        }
        public static string CreateCaseNode(string caseNo, string summary, string tags, string nodeVariable)
        {
            return "MERGE (" + nodeVariable + ":SeibelCase {sr: \"" + caseNo + "\", summary: \"" + summary + "\", tags: [" + tags + "]})";
        }
        public static string CreateCategoryNode(string category, string aliases)
        {
            return "MERGE (:Category {umbrellaterm: \"" + category + "\", aliases: [" + aliases + "]})";
        }
        public static string CreateCategoryNode(string category, string aliases, string nodeVariable)
        {
            return "MERGE (" + nodeVariable + ":Category {umbrellaterm: \"" + category + "\", aliases: [" + aliases + "]})";
        }
        public static string CreateProductNode(string product)
        {
            return "MERGE (:Product {name: \"" + product + "\"})";
        }
        public static string CreateProductNode(string product, string nodeVariable)
        {
            return "MERGE (" + nodeVariable + ":Product {name: \"" + product + "\"})";
        }
        // public string MakeRelationshipBetween(string node1, string node2, string relationship, Dictionary<string, string> relationshipProps)
        // {
        //     return relationshipProps.Count == 0
        //     ? "MERGE (" + node1 + ") -[:" + relationship + "]-> (" + node2 + ")"
        //     : "MERGE (" + node1 + ") -[:" + relationship + " { " + string.Join(", ", relationshipProps.Select(kvp => $"{kvp.Key}: \"{kvp.Value}\"")) + "}]-> (" + node2 + ")";
        // }
        public static string MakeRelationshipBetweenNodes(string node1, string node2, string relationship)
        {
            return "MERGE (" + node1 + ") -[:" + relationship + "]-> (" + node2 + ")";
        }
        public static string MakeRelationshipBetweenNodesWithProps(string node1, string node2, string relationship, string relationshipProps)
        {
            return "MERGE (" + node1 + ") -[:" + relationship + " { " + relationshipProps + "}]-> (" + node2 + ")";
        }
        public static void InsertData(
            string boltConnection,
            string username,
            string password,
            Dictionary<string, List<CaseFormat>> dictionary,
            Product seibelJSON)
        {
            var insertForEvery = 100;
            var areaCatQuery = new StringBuilder();
            var query = new StringBuilder();
            var areasDict = new Dictionary<string, string>() { };
            var catsDict = new Dictionary<string, string>() { };
            var caseDict = new Dictionary<string, string>() { };
            var node_number = 0;
            using (var graph = new GraphQuery(boltConnection, username, password))
            {
                areaCatQuery.AppendLine(CreateProductNode(seibelJSON.productName, "product"));
                foreach (var area in seibelJSON.areas)
                {
                    var areaVariable = "area" + ++node_number;
                    areasDict.Add(area.area.ToLower(), areaVariable);
                    areaCatQuery.AppendLine(CreateAreaNode(area.area, areaVariable));
                    areaCatQuery.AppendLine(MakeRelationshipBetweenNodes(areaVariable, "product", "BELONGS_TO"));

                    foreach (var cat in area.categories)
                    {
                        var aliases = string.Join(", ", cat.aliases.Select(alias => $"\"{alias}\""));
                        var catVariable = "cat" + ++node_number;
                        catsDict.Add(cat.umbrellaterm.ToLower(), catVariable);
                        areaCatQuery.AppendLine(CreateCategoryNode(cat.umbrellaterm, aliases, catVariable));
                        areaCatQuery.AppendLine(MakeRelationshipBetweenNodes(catVariable, areaVariable, "BELONGS_TO"));
                    }
                }

                // dictionary = {
                //     "SR#": [
                //         { // Category  tags
                //             "tags": ["ssp", "payment", "scheme"],
                //             "categoryAndConnectionType": {
                //                 "sick": ["payroll", "setup"]
                //             }
                //         },
                //         { // Category  tags
                //             "tags": ["annual"], 
                //             "categoryAndConnectionType": {
                //                 "annual": ["DIRECT"]
                //             }
                //         }
                //     ]
                // }
                // AnalyzeSummary.SeibelSummary = {
                //     "SR#": "Summary"
                // }

                var sr_count = 0;
                var total_sr_count = dictionary.Count;
                foreach (var item in dictionary)
                {
                    sr_count++;
                    if (sr_count % insertForEvery == 0) {
                        var q = areaCatQuery.AppendLine(query.ToString()).ToString();
                        graph.RunQuery(q);
                        query = new StringBuilder();
                    }
                    // item.Key -> SR#
                    // item.Value -> [{tags, categoryAndConnectionType}]
                    var summary = AnalyzeSummary.SeibelSummary[item.Key].Replace("\\", " ");
                    // System.Console.WriteLine($"{item.Key} :: {AnalyzeSummary.SeibelSummary[item.Key]}");

                    var tags = item.Value.SelectMany(csf => csf.tags.Select(tag => $"\"{tag}\"")); // ["ssp", "payment", "scheme", "annual"]
                    var catsWithConnectionType = item.Value.SelectMany(csf => csf.categoryAndConnectionType); // [ {"sick", ["payroll", "setup"]}, {"annual", ["DIRECT"]} ]

                    var caseVariable = "case" + ++node_number;
                    caseDict.Add(item.Key, caseVariable);
                    query.AppendLine(CreateCaseNode(item.Key, summary, string.Join(", ", tags), caseVariable));

                    foreach (var cat in catsWithConnectionType)
                    {
                        var catVariable = catsDict[cat.Key];
                        if (cat.Value[0] == "DIRECT")
                        {
                            query.AppendLine(MakeRelationshipBetweenNodes(
                                caseVariable,
                                catVariable,
                                "BELONGS_TO"
                            ));
                        }
                        else
                        {
                            query.AppendLine(MakeRelationshipBetweenNodesWithProps(
                                caseVariable,
                                catVariable,
                                "BELONGS_TO",
                                "subCategories:[" + string.Join(", ", cat.Value.Select(subcat => $"\"{subcat}\"")) + "]"
                            ));
                        }
                    }
                }

                // graph.RunQuery(query.ToString());
                graph.RunQuery(areaCatQuery.AppendLine(query.ToString()).ToString());
            }
        }
    }
}