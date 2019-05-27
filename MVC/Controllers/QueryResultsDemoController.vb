Imports System.Data
Imports System.Linq
Imports System.Net
Imports System.Web.Mvc
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Core.QueryTransformer
Imports MVC_Samples.Helpers
Imports ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class QueryResultsDemoController
		Inherits Controller
		Private instanceId As String = "QueryResults"

		Public Function Index() As ActionResult
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get](instanceId)
			Dim qt = QueryTransformerStore.[Get](instanceId)

			If qb Is Nothing Then
				qb = CreateQueryBuilder()
			End If

			If qt Is Nothing Then
				qt = CreateQueryTransformer(qb.SQLQuery)
			End If

			ViewBag.QueryTransformer = qt

			Return View(qb)
		End Function

		Public Function GetData(m As GridModel) As ActionResult
			Dim qt = QueryTransformerStore.[Get](instanceId)

			qt.Skip((m.Pagenum * m.Pagesize).ToString())
			qt.Take(If(m.Pagesize = 0, "", m.Pagesize.ToString()))

			If Not String.IsNullOrEmpty(m.Sortdatafield) Then
				qt.Sortings.Clear()

				If Not String.IsNullOrEmpty(m.Sortorder) Then
					Dim c = qt.Columns.FindColumnByResultName(m.Sortdatafield)
					qt.OrderBy(c, m.Sortorder.ToLower() = "asc")
				End If
			End If

			Return GetData(qt, m.Params)
		End Function

		Private Function GetData(qt As QueryTransformer, _params As Param()) As ActionResult
			Dim conn = qt.Query.SQLContext.MetadataProvider.Connection
			Dim sql = qt.SQL

			If _params IsNot Nothing Then
				For Each p As Param In _params
					p.DataType = qt.Query.QueryParameters.First(Function(qp) qp.FullName = p.Name).DataType
				Next
			End If

			Try
				Dim data = DataBaseHelper.GetData(conn, sql, _params)
				Return Json(data, JsonRequestBehavior.AllowGet)
			Catch e As Exception
				Return Json(New ErrorOutput(e.Message), JsonRequestBehavior.AllowGet)
			End Try
		End Function

        Private Class ErrorOutput
            Public ErrorText as String

            Public Sub New(errorText As String) 
                Me.ErrorText = errorText
            End Sub
        End Class

		''' <summary>
		''' Creates and initializes a new instance of the QueryBuilder object.
		''' </summary>
		''' <returns>Returns instance of the QueryBuilder object.</returns>
		Private Function CreateQueryBuilder() As QueryBuilder
			' Create an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.Factory.SqLite(instanceId)

			' Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
			queryBuilder.BehaviorOptions.AllowSleepMode = False
            
			' Bind Active Query Builder to a live database connection.
				' Assign an instance of DBConnection object to the Connection property.
			queryBuilder.MetadataProvider = New SQLiteMetadataProvider() With { _
				.Connection = DataBaseHelper.CreateSqLiteConnection("SqLiteDataBase") _
			}

			' Assign the initial SQL query text the user sees on the _first_ page load
			queryBuilder.SQL = GetDefaultSql()

			Return queryBuilder
		End Function

		Private Function GetDefaultSql() As String
			Return "Select customers.CustomerId," & vbCr & vbLf & "                      customers.LastName," & vbCr & vbLf & "                      customers.FirstName" & vbCr & vbLf & "                    From customers"
		End Function

		''' <summary>
		''' Creates and initializes a new instance of the QueryTransformer object.
		''' </summary>
		''' <param name="query">SQL Query to transform.</param>
		''' <returns>Returns instance of the QueryTransformer object.</returns>
		Private Function CreateQueryTransformer(query As SQLQuery) As QueryTransformer
			Dim qt = QueryTransformerStore.Create(instanceId)

			qt.QueryProvider = query
			qt.AlwaysExpandColumnsInQuery = True

			Return qt
		End Function
	End Class

	Public Class GridModel
		Public Property Pagenum() As Integer
			Get
				Return m_Pagenum
			End Get
			Set
				m_Pagenum = Value
			End Set
		End Property
		Private m_Pagenum As Integer
		Public Property Pagesize() As Integer
			Get
				Return m_Pagesize
			End Get
			Set
				m_Pagesize = Value
			End Set
		End Property
		Private m_Pagesize As Integer
		Public Property Sortdatafield() As String
			Get
				Return m_Sortdatafield
			End Get
			Set
				m_Sortdatafield = Value
			End Set
		End Property
		Private m_Sortdatafield As String
		Public Property Sortorder() As String
			Get
				Return m_Sortorder
			End Get
			Set
				m_Sortorder = Value
			End Set
		End Property
		Private m_Sortorder As String
		Public Property Params() As Param()
			Get
				Return m_Params
			End Get
			Set
				m_Params = Value
			End Set
		End Property
		Private m_Params As Param()
	End Class

	Public Class Param
		Public Property Name() As String
			Get
				Return m_Name
			End Get
			Set
				m_Name = Value
			End Set
		End Property
		Private m_Name As String
		Public Property Value() As String
			Get
				Return m_Value
			End Get
			Set
				m_Value = Value
			End Set
		End Property
		Private m_Value As String
		Public Property DataType() As DbType
			Get
				Return m_DataType
			End Get
			Set
				m_DataType = Value
			End Set
		End Property
		Private m_DataType As DbType
	End Class
End Namespace
