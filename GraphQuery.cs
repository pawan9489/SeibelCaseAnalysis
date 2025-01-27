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
        public static string MergeAreaNode(string area)
        {
            return "MERGE (:Area {name: \"" + area + "\"})";
        }
        public static string MergeAreaNode(string area, string nodeVariable)
        {
            return "MERGE (" + nodeVariable + ":Area {name: \"" + area + "\"})";
        }
        public static string CreateCaseNode(string caseNo, string summary, string tags, string status, string priority, string date_created, string group, string product_name, string original_product_name)
        {
            return "CREATE (:SeibelCase {sr: \"" + caseNo + "\", summary: \"" + summary + "\", tags: [" + tags + "], status: \"" + status + "\", priority: \"" + priority + "\", date_created: datetime(\"" + date_created + "\"), group: \"" + group + "\", product_name: \"" + product_name + "\", original_product_name: \"" + original_product_name + "\"})";
        }
        public static string CreateCaseNode(string caseNo, string summary, string tags, string status, string priority, string date_created, string group, string product_name, string original_product_name, string nodeVariable)
        {
            return "CREATE (" + nodeVariable + ":SeibelCase {sr: \"" + caseNo + "\", summary: \"" + summary + "\", tags: [" + tags + "], status: \"" + status + "\", priority: \"" + priority + "\", date_created: datetime(\"" + date_created + "\"), group: \"" + group + "\", product_name: \"" + product_name + "\", original_product_name: \"" + original_product_name + "\"})";
        }
        public static string MergeCaseNode(string caseNo, string summary, string tags, string status, string priority, string date_created)
        {
            return "MERGE (:SeibelCase {sr: \"" + caseNo + "\", summary: \"" + summary + "\", tags: [" + tags + "], status: \"" + status + "\", priority: \"" + priority + "\", date_created: datetime(\"" + date_created + "\")})";
        }
        public static string MergeCaseNode(string caseNo, string summary, string tags, string status, string priority, string date_created, string nodeVariable)
        {
            return "MERGE (" + nodeVariable + ":SeibelCase {sr: \"" + caseNo + "\", summary: \"" + summary + "\", tags: [" + tags + "], status: \"" + status + "\", priority: \"" + priority + "\", date_created: datetime(\"" + date_created + "\")})";
        }
        public static string MergeCategoryNode(string category, string aliases)
        {
            return "MERGE (:Category {umbrellaterm: \"" + category + "\", aliases: [" + aliases + "]})";
        }
        public static string MergeCategoryNode(string category, string aliases, string nodeVariable)
        {
            return "MERGE (" + nodeVariable + ":Category {umbrellaterm: \"" + category + "\", aliases: [" + aliases + "]})";
        }
        public static string MergeProductNode(string product)
        {
            return "MERGE (:Product {name: \"" + product + "\"})";
        }
        public static string MergeProductNode(string product, string nodeVariable)
        {
            return "MERGE (" + nodeVariable + ":Product {name: \"" + product + "\"})";
        }
        public static string MergeRelationshipBetweenNodes(string node1, string node2, string relationship)
        {
            return "MERGE (" + node1 + ") -[:" + relationship + "]-> (" + node2 + ")";
        }
        public static string MergeRelationshipBetweenNodesWithProps(string node1, string node2, string relationship, string relationshipProps)
        {
            return "MERGE (" + node1 + ") -[:" + relationship + " { " + relationshipProps + "}]-> (" + node2 + ")";
        }
        public static string CreateRelationshipBetweenNodes(string node1, string node2, string relationship)
        {
            return "CREATE (" + node1 + ") -[:" + relationship + "]-> (" + node2 + ")";
        }
        public static string CreateRelationshipBetweenNodesWithProps(string node1, string node2, string relationship, string relationshipProps)
        {
            return "CREATE (" + node1 + ") -[:" + relationship + " { " + relationshipProps + "}]-> (" + node2 + ")";
        }
        public static void InsertData(
            string boltConnection,
            string username,
            string password,
            Dictionary<string, List<CaseFormat>> dictionary,
            Product seibelJSON)
        {
            var areaCatQuery = new StringBuilder();
            var query = new StringBuilder();
            var areasDict = new Dictionary<string, string>() { };
            var catsDict = new Dictionary<string, string>() { };
            var caseDict = new Dictionary<string, string>() { };
            var node_number = 0;
            using (var graph = new GraphQuery(boltConnection, username, password))
            {
                areaCatQuery.AppendLine(MergeProductNode(seibelJSON.productName, "product"));
                foreach (var area in seibelJSON.areas)
                {
                    var areaVariable = "area" + ++node_number;
                    areasDict.Add(area.area.ToLower(), areaVariable);
                    areaCatQuery.AppendLine(MergeAreaNode(area.area, areaVariable));
                    areaCatQuery.AppendLine(MergeRelationshipBetweenNodes(areaVariable, "product", "BELONGS_TO"));

                    foreach (var cat in area.categories)
                    {
                        var aliases = string.Join(", ", cat.aliases.Select(alias => $"\"{alias}\""));
                        var catVariable = "cat" + ++node_number;
                        catsDict.Add(cat.umbrellaterm.ToLower(), catVariable);
                        areaCatQuery.AppendLine(MergeCategoryNode(cat.umbrellaterm, aliases, catVariable));
                        areaCatQuery.AppendLine(MergeRelationshipBetweenNodes(catVariable, areaVariable, "BELONGS_TO"));
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

                foreach (var item in dictionary)
                {
                    // item.Key -> SR#
                    // item.Value -> [{tags, categoryAndConnectionType}]
                    var summary = AnalyzeSummary.SeibelSummary[item.Key]["summary"].Replace("\\", " ");
                    var status = AnalyzeSummary.SeibelSummary[item.Key]["status"];
                    var priority = AnalyzeSummary.SeibelSummary[item.Key]["priority"];
                    var date_created = AnalyzeSummary.SeibelSummary[item.Key]["date created"];
                    var group = AnalyzeSummary.SeibelSummary[item.Key]["group"];
                    var product_name = AnalyzeSummary.SeibelSummary[item.Key]["product name"];
                    var original_product_name = AnalyzeSummary.SeibelSummary[item.Key]["original product name"];

                    var tags = item.Value.SelectMany(csf => csf.tags.Select(tag => $"\"{tag}\"")); // ["ssp", "payment", "scheme", "annual"]
                    var catsWithConnectionType = item.Value.SelectMany(csf => csf.categoryAndConnectionType); // [ {"sick", ["payroll", "setup"]}, {"annual", ["DIRECT"]} ]

                    var caseVariable = "case" + ++node_number;
                    caseDict.Add(item.Key, caseVariable);
                    query.AppendLine(CreateCaseNode(item.Key, summary, string.Join(", ", tags), status, priority, date_created, group, product_name, original_product_name, caseVariable));

                    foreach (var cat in catsWithConnectionType)
                    {
                        var catVariable = catsDict[cat.Key];
                        if (cat.Value[0] == "DIRECT")
                        {
                            query.AppendLine(CreateRelationshipBetweenNodes(
                                caseVariable,
                                catVariable,
                                "BELONGS_TO"
                            ));
                        }
                        else
                        {
                            query.AppendLine(CreateRelationshipBetweenNodesWithProps(
                                caseVariable,
                                catVariable,
                                "BELONGS_TO",
                                "subCategories:[" + string.Join(", ", cat.Value.Select(subcat => $"\"{subcat}\"")) + "]"
                            ));
                        }
                    }
                }
                Console.WriteLine(areaCatQuery.ToString() + query.ToString());
                // graph.RunQuery(areaCatQuery.ToString() + query.ToString());
            }
        }
    }
}