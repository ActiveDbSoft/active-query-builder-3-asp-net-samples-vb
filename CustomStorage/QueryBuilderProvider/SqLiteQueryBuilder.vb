Imports System.Data
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace QueryBuilderProvider
	Public Class SqLiteQueryBuilder
		Inherits QueryBuilder
		Public Sub New(connection As IDbConnection, instanceId As String)
			MyBase.New(instanceId)
			' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
			BehaviorOptions.AllowSleepMode = False

			' Assign an instance of the syntax provider which defines SQL syntax and metadata retrieval rules.
			SyntaxProvider = New SQLiteSyntaxProvider()

			' Bind Active Query Builder to a live database connection.
				' Assign an instance of DBConnection object to the Connection property.
			MetadataProvider = New SQLiteMetadataProvider() With { _
				.Connection = connection _
			}
		End Sub
	End Class
End Namespace
