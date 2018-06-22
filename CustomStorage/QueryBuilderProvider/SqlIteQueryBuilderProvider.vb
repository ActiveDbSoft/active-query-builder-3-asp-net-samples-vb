Imports System.Data
Imports ActiveQueryBuilder.Web.Server
Imports ActiveQueryBuilder.Web.Server.Infrastructure.Providers
Imports CustomStorage.Helpers

Namespace QueryBuilderProvider
	''' <summary>
	''' QueryBuilder storage provider which saves the state in Sqlite database
	''' </summary>
	Public Class QueryBuilderSqliteStoreProvider
		Implements IQueryBuilderProvider
	    Public ReadOnly Property SaveState As Boolean Implements IAQBProvider.SaveState
            Get
                Return True
            End Get
	    End Property
		''' <summary>
		''' Connection to the Sqlite database
		''' </summary>
		Private ReadOnly _connection As IDbConnection

        Public Sub New()
			_connection = DataBaseHelper.CreateSqLiteConnection("SqLiteDataBase")

			Dim sql = "create table if not exists QueryBuilders(id text primary key, layout TEXT)"
			ExecuteNonQuery(sql)
		End Sub

		''' <summary>
		''' Creates an instance of the QueryBuilder object and loads its state identified by the given id.
		''' </summary>
		''' <param name="id">The identifier.</param>
		''' <returns></returns>
		Public Function [Get](id As String) As QueryBuilder Implements IQueryBuilderProvider.Get
			Dim qb = New SqLiteQueryBuilder(_connection, id)

			Dim layout = GetLayout(id)

			If layout IsNot Nothing Then
				qb.LayoutSQL = layout
			End If

			Return qb
		End Function

		''' <summary>
		''' Saves the state of QueryBuilder object identified by its Tag property.
		''' </summary>
		''' <param name="qb">The QueryBuilder object.</param>
		Public Sub Put(qb As QueryBuilder) Implements IQueryBuilderProvider.Put
			If GetLayout(qb.Tag) Is Nothing Then
				Insert(qb)
			Else
				Update(qb)
			End If
		End Sub

		''' <summary>
		''' Clears the state of QueryBuilder object identified by the given id.
		''' </summary>
		''' <param name="id">The identifier.</param>
		Public Sub Delete(id As String) Implements IQueryBuilderProvider.Delete
			Dim sql = String.Format("delete from QueryBuilders where id = {0}", id)
			ExecuteNonQuery(sql)
		End Sub

		Private Sub Insert(qb As QueryBuilder)
			Dim sql = String.Format("insert into QueryBuilders values ('{0}', '{1}')", qb.Tag, qb.LayoutSQL)
			ExecuteNonQuery(sql)
		End Sub
		Private Sub Update(qb As QueryBuilder)
			Dim sql = String.Format("update QueryBuilders set layout = '{1}' where id = '{0}'", qb.Tag, qb.LayoutSQL)
			ExecuteNonQuery(sql)
		End Sub

		Protected Sub ExecuteNonQuery(sql As String)
			Try
				If _connection.State <> ConnectionState.Open Then
					_connection.Open()
				End If

				Using cmd = CreateCommand(sql)
					cmd.ExecuteNonQuery()
				End Using
			Finally
				_connection.Close()
			End Try
		End Sub

		Protected Function GetLayout(id As String) As String
			Dim sql = String.Format("select layout from QueryBuilders where id = '{0}'", id)

			Try
				If _connection.State <> ConnectionState.Open Then
					_connection.Open()
				End If

				Using cmd = CreateCommand(sql)
					Using reader = cmd.ExecuteReader()
						If reader.Read() Then
							Return reader("layout").ToString()
						End If
					End Using
				End Using

				Return Nothing
			Finally
				_connection.Close()
			End Try
		End Function

		Protected Function CreateCommand(sql As String) As IDbCommand
			Dim cmd = _connection.CreateCommand()
			cmd.CommandText = sql
			Return cmd
		End Function
	End Class

	''' <summary>
	''' Sample of the QueryBuilder storage provider for Redis (https://redis.io/)
	''' </summary>
	' public class QueryBuilderRedisStoreProvider : IQueryBuilderProvider
'    {
'        private readonly IDatabase _db;
'
'        public RedisQueryBuilderProvider()
'        {
'            var redis = ConnectionMultiplexer.Connect();
'            _db = redis.GetDatabase();
'        }
'
'        public QueryBuilder Get(string id)
'        {
'            var layout = _db.StringGet(id);
'
'            var qb = new QueryBuilder(id);
'
'            if (layout.HasValue)
'                qb.LayoutSQL = layout;
'
'            return qb;
'        }
'
'        public void Put(QueryBuilder qb)
'        {
'            _db.StringSetAsync(qb.Tag, qb.LayoutSQL);
'        }
'
'        public void Delete(string id)
'        {
'            _db.StringSetAsync(id, "");
'        }
'    }

End Namespace
