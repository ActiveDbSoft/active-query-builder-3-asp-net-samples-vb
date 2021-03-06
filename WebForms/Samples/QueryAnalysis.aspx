﻿<%@ Page Title="Query Analysis Demo" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="QueryAnalysis.aspx.cs" Inherits="WebForms_Samples.Samples.QueryAnalysis" %>
<%@ Register TagPrefix="AQB" Namespace="ActiveQueryBuilder.Web.WebForms" Assembly="ActiveQueryBuilder.Web.WebForms" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-12">
            <h1>Query Analysis Demo</h1>
            <p>Explore the internal query object model, get summary information about the query.</p>
            <div class="header block-flat">
                <div id="sample-selector" class="qb-ui-syntax-selector">
                    <span class="qb-ui-syntax-selector-label">Load sample queries:</span>
                    <input id="sample-1" type="button" value="SELECT" />&nbsp;
                <input id="sample-2" type="button" value="SELECT FROM WHERE" />&nbsp;
                <input id="sample-3" type="button" value="SELECT FROM JOIN" />&nbsp;
                <input id="sample-4" type="button" value="SELECT FROM with subqueries" />&nbsp;
                <input id="sample-5" type="button" value="MULTIPLE UNIONs" />
                </div>
            </div>
        </div>
        <div class="col-md-12">
            <!--Turn the UseDefaultTheme to False for not using the default theme. You will have to load the JQueryUI library then. -->
            <AQB:QueryBuilderControl ID="QueryBuilderControl1" runat="server" UseDefaultTheme="false" />
            <div class="qb-ui-layout">
                <div class="qb-ui-layout__top">
                    <div class="qb-ui-layout__left">
                        <div class="qb-ui-structure-tabs">
                            <div class="qb-ui-structure-tabs__tab">
                                <input type="radio" id="tree-tab" name="qb-tabs" checked />
                                <label class="ui-widget-header qb-widget-header" for="tree-tab">Database</label>
                                <div class="qb-ui-structure-tabs__content">
                                    <AQB:ObjectTreeView runat="server" ID="ObjectTreeView1" />
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="qb-ui-layout__right">
                        <AQB:SubQueryNavigationBar runat="server" ID="SubQueryNavigationBar1" />
                        <AQB:Canvas runat="server" ID="Canvas1" />
                        <AQB:StatusBar runat="server" ID="StatusBar1" />
                        <AQB:Grid runat="server" ID="Grid1" />
                    </div>
                </div>
                <div class="qb-ui-layout__bottom">
                    <div id="main-tabs" class="block-flat">
                        <ul>
                            <li><a href="#qb-ui-editor">SQL</a></li>
                            <li><a href="#statistics">Statistics</a></li>
                            <li><a href="#sub-queries">SubQueries</a></li>
                            <li><a href="#query-structure">Query structure</a></li>
                            <li><a href="#union-sub-query">UnionSubQuery</a></li>
                        </ul>
                        <div id="qb-ui-editor">
                            <AQB:SqlEditor runat="server" ID="SqlEditor1" />
                        </div>
                        <div id="statistics"></div>
                        <div id="sub-queries"></div>
                        <div id="query-structure"></div>
                        <div id="union-sub-query">
                            <div id="union-sub-query-tabs">
                                <ul>
                                    <li><a href="#selected-expressions">Selected Expressions</a></li>
                                    <li><a href="#datasources">DataSources</a></li>
                                    <li><a href="#links">Links</a></li>
                                    <li><a href="#where">Where</a></li>
                                </ul>
                                <div id="selected-expressions"></div>
                                <div id="datasources"></div>
                                <div id="links"></div>
                                <div id="where"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>


    <style>
        #alternate-sql {
            border-style: solid;
            border-color: #DDDDDD;
            border-width: 1px;
            display: block;
            position: relative;
            width: 100%;
            overflow: auto;
            top: 0;
            left: 0;
        }

            #alternate-sql textarea {
                width: 100%;
                height: 150px;
                padding: 0;
                border: 0;
                margin: 0;
            }

        .sql {
            position: relative;
            padding: 0;
            width: 100%;
        }

        #qb-ui-canvas {
            height: 230px;
        }

        #qb-ui-grid {
            height: 180px;
        }


        .qb-ui-syntax-selector {
            font-family: Verdana, Arial, sans-serif;
            font-size: 11px;
            height: auto;
        }

        .qb-ui-syntax-selector-label {
            padding-left: 5px;
        }

        .qb-ui-syntax-selector-version-label {
            margin-left: 20px;
        }

        .header {
            height: auto;
            padding: 5px;
        }

        #qb-ui-editor {
            border: 0;
            box-shadow: none;
            margin: 0;
        }

        .ui-tabs {
            border: 0;
        }

        #qb-ui-editor textarea {
            height: 100px;
        }

        #alternate-sql {
            border-style: solid;
            border-color: #DDDDDD;
            border-width: 1px;
            display: block;
            position: relative;
            width: 100%;
            overflow: auto;
            top: 0;
            left: 0;
        }

            #alternate-sql textarea {
                width: 100%;
                height: 100px;
                padding: 0;
                border: 0;
                margin: 0;
            }

        .sql {
            position: relative;
            padding: 0;
            width: 100%;
        }

        #qb-ui-canvas {
            height: 244px;
        }


        .ui-tabs {
            padding: 0;
        }

            .ui-tabs .ui-tabs-nav {
                border-left: 0;
                border-right: 0;
                border-top: 0;
                background: none;
                font-weight: bold;
                font-size: 13px;
            }

                .ui-tabs .ui-tabs-nav li a {
                    padding: 3px 10px;
                    outline: none;
                }

        #qb-ui-editor textarea {
            height: 150px;
        }

        .ui-tabs .ui-tabs-panel {
            padding: 0;
            height: 150px;
            overflow: auto;
        }

        #union-sub-query {
            height: 180px;
        }

        .ui-tabs .ui-tabs-panel textarea, .ui-tabs .ui-tabs-panel {
            font-family: Courier New;
            font-size: 13px;
        }
    </style>

    <script src="https://code.jquery.com/ui/1.12.0/jquery-ui.min.js"></script>
    <script>
        var SampleSQL = {
            "sample-1":
            "Select 1 as cid, Upper('2'), 3, 4 + 1, 5 + 2 IntExpression ",
            "sample-2":
            "Select c.ID As cid, c.Company, Upper(c.Company), o.Order_ID\n" +
            "From Customers c Inner Join\n" +
            "  Orders o On c.ID = o.Customer_ID\n" +
            "Where c.ID < 10 And o.Order_ID > 0",
            "sample-3":
            "Select c.ID As cid, Upper(c.Company), o.Order_ID + 1, p.Product_Name,\n" +
            "  2 + 2 IntExpression\n" +
            "From Customers c Inner Join\n" +
            "  Orders o On c.ID = o.Customer_ID Inner Join\n" +
            "  Order_Details od On o.Order_ID = od.Order_ID Inner Join\n" +
            "  Products p On p.ID = od.Product_ID",
            "sample-4":
            "Select c.ID As cid, Upper(c.Company), o.Order_ID + 1, p.Product_Name,\n" +
            "  2 + 2 IntExpression\n" +
            "From Customers c Inner Join\n" +
            "  Orders o On c.ID = o.Customer_ID Inner Join\n" +
            "  Order_Details od On o.Order_ID = od.Order_ID Inner Join\n" +
            "  (Select pr.Product_ID, pr.ProductName\n" +
            "    From Products pr) p On p.ID = od.Product_ID",
            "sample-5":
            "Select c.ID As cid, Upper(c.Company), o.Order_ID + 1, p.Product_Name,\n" +
            "  2 + 2 IntExpression\n" +
            "From Customers c Inner Join\n" +
            "  Orders o On c.ID = o.Customer_ID Inner Join\n" +
            "  Order_Details od On o.Order_ID = od.Order_ID Inner Join\n" +
            "  (Select pr.Product_ID, pr.ProductName\n" +
            "    From Products pr) p On p.ID = od.Product_ID\n" +
            "Union All\n" +
            "(Select 1, 2, 3, 4, 5\n" +
            "Union All\n" +
            "Select 6, 7, 8, 9, 0)\n" +
            "Union All\n" +
            "Select (Select Null As [Null]) As EmptyValue, SecondColumn = 2,\n" +
            "  Lower('ThirdColumn') As ThirdColumn, 0 As [Quoted Alias], 2 + 2 * 2"
        }

        $(function () {
            $("#main-tabs").tabs();
            $("#union-sub-query-tabs").tabs();

            $("#sample-selector input").on('click', function (e) {
                AQB.Web.QueryBuilder.setSql(SampleSQL[e.target.id]);
            });

            AQB.Web.onApplicationReady(function () {
                AQB.Web.Core.on(AQB.Web.Core.Events.UserDataReceived, onUserDataReceived);
            });

            function onUserDataReceived(data) {
                $('#statistics').html(data.Statistics);
                $('#sub-queries').html(data.SubQueries);
                $('#query-structure').html(data.QueryStructure);
                $('#selected-expressions').html(data.UnionSubQuery.SelectedExpressions);
                $('#datasources').html(data.UnionSubQuery.DataSources);
                $('#links').html(data.UnionSubQuery.Links);
                $('#where').html(data.UnionSubQuery.Where);
            };
        });
    </script>

</asp:Content>
