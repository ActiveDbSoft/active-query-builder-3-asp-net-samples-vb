Imports System.IO
Imports System.Web
Imports System.Web.SessionState
Imports ActiveQueryBuilder.Core.QueryTransformer
Imports ActiveQueryBuilder.Web.Server
Imports WebForms_Samples.Helpers

Namespace Handlers
	Public Class QueryResults
		Implements IHttpHandler
		Implements IRequiresSessionState
		''' <summary>
		''' You will need to configure this handler in the Web.config file of your 
		''' web and register it with IIS before being able to use it. For more information
		''' see the following link: https://go.microsoft.com/?linkid=8101007
		''' </summary>
		#Region "IHttpHandler Members"

		Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
			' Return false in case your Managed Handler cannot be reused for another request.
			' Usually this would be false in case you have some state information preserved per request.
			Get
				Return True
			End Get
		End Property

		Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
			Dim model = If(CreateFromPostData(context.Request), CreateFromGetParams(context.Request))
			Dim result = GetData(model)
			Dim content = Newtonsoft.Json.JsonConvert.SerializeObject(result)

			context.Response.Write(content)
		End Sub

		Private Function CreateFromGetParams(r As HttpRequest) As GridModel
			'filterscount=0&groupscount=0&=0&=10&recordstartindex=0&recordendindex=10
			Return New GridModel() With { _
				.Pagenum = Integer.Parse(r.Params("pagenum")), _
				.Pagesize = Integer.Parse(r.Params("pagesize")), _
				.Sortdatafield = r.Params("sortdatafield"), _
				.Sortorder = r.Params("sortorder") _
			}
		End Function

		Private Function CreateFromPostData(r As HttpRequest) As GridModel
			Dim input As String = New StreamReader(r.InputStream).ReadToEnd()
			Return Newtonsoft.Json.JsonConvert.DeserializeObject(Of GridModel)(input)
		End Function

		Public Shared Function GetData(m As GridModel) As Object
			Dim qt = QueryTransformerStore.[Get]("QueryResults")

			qt.Skip((m.Pagenum * m.Pagesize).ToString())
			qt.Take(If(m.Pagesize = 0, "", m.Pagesize.ToString()))

			If Not String.IsNullOrEmpty(m.Sortdatafield) Then
				qt.Sortings.Clear()

				If Not String.IsNullOrEmpty(m.Sortorder) Then
					Dim c = qt.Columns.FindColumnByResultName(m.Sortdatafield)
					qt.OrderBy(c, m.Sortorder.ToLower() = "asc")
				End If
			End If

			Return GetData(qt)
		End Function

		Private Shared Function GetData(qt As QueryTransformer) As Object
			Dim conn = qt.Query.SQLContext.MetadataProvider.Connection
			Dim sql = qt.SQL

			Return DataBaseHelper.GetData(conn, sql)
		End Function

		#End Region
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
	End Class
End Namespace
