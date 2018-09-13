Imports System.Data
Imports System.IO
Imports System.Web.Mvc
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server
Imports MVC_Samples.Helpers

Namespace Controllers
	Public Class LoadMetadataDemoController
		Inherits Controller
		Private ReadOnly _conn As IDbConnection = DataBaseHelper.CreateSqLiteConnection("SqLiteDataBase")

		Public Function Index() As ActionResult
			' Get an instance of the QueryBuilder object
			Dim qb = QueryBuilderStore.[Get]("LoadMetadata")

			If qb Is Nothing Then
				qb = QueryBuilderStore.Create("LoadMetadata")
				qb.SyntaxProvider = New GenericSyntaxProvider()
			End If

			Return View(qb)
		End Function

		'''///////////////////////////////////////////////////////////////////////
		''' 1st way:
		''' This method demonstrates the direct access to the internal metadata 
		''' objects collection (MetadataContainer).
		'''///////////////////////////////////////////////////////////////////////
		Public Sub Way1()
			Dim queryBuilder1 = QueryBuilderStore.[Get]("LoadMetadata")

			ResetQueryBuilderMetadata(queryBuilder1)
			queryBuilder1.SyntaxProvider = New GenericSyntaxProvider()
			' prevent QueryBuilder to request metadata
			queryBuilder1.MetadataLoadingOptions.OfflineMode = True

			queryBuilder1.MetadataProvider = Nothing

			Dim metadataContainer As MetadataContainer = queryBuilder1.MetadataContainer
			metadataContainer.BeginUpdate()

			Try
				metadataContainer.Clear()

				Dim schemaDbo As MetadataNamespace = metadataContainer.AddSchema("dbo")

				' prepare metadata for table "Orders"
				Dim orders As MetadataObject = schemaDbo.AddTable("Orders")
				' fields
				orders.AddField("OrderId")
				orders.AddField("CustomerId")

				' prepare metadata for table "Order Details"
				Dim orderDetails As MetadataObject = schemaDbo.AddTable("Order Details")
				' fields
				orderDetails.AddField("OrderId")
				orderDetails.AddField("ProductId")
				' foreign keys
				Dim foreignKey As MetadataForeignKey = orderDetails.AddForeignKey("OrderDetailsToOrders")

				Using referencedName As New MetadataQualifiedName()
					referencedName.Add("Orders")
					referencedName.Add("dbo")

					foreignKey.ReferencedObjectName = referencedName
				End Using

				foreignKey.Fields.Add("OrderId")
				foreignKey.ReferencedFields.Add("OrderId")
			Finally
				metadataContainer.EndUpdate()
			End Try
		End Sub

		'''///////////////////////////////////////////////////////////////////////
		''' 2rd way:
		''' This method demonstrates on-demand manual filling of metadata structure using 
		''' corresponding MetadataContainer.ItemMetadataLoading event
		'''///////////////////////////////////////////////////////////////////////
		Public Sub Way2()
			Dim queryBuilder1 = QueryBuilderStore.[Get]("LoadMetadata")
			ResetQueryBuilderMetadata(queryBuilder1)
			' allow QueryBuilder to request metadata
			queryBuilder1.MetadataLoadingOptions.OfflineMode = False

			queryBuilder1.MetadataProvider = Nothing
			Dim mc As MetadataContainer = queryBuilder1.MetadataContainer
			AddHandler mc.ItemMetadataLoading, AddressOf way2ItemMetadataLoading
		End Sub

		Private Sub way2ItemMetadataLoading(sender As Object, item As MetadataItem, types As MetadataType)
			Select Case item.Type
				Case MetadataType.Root
					If (types And MetadataType.Schema) > 0 Then
						item.AddSchema("dbo")
					End If
					Exit Select

				Case MetadataType.Schema
					If (item.Name = "dbo") AndAlso (types And MetadataType.Table) > 0 Then
						item.AddTable("Orders")
						item.AddTable("Order Details")
					End If
					Exit Select

				Case MetadataType.Table
					If item.Name = "Orders" Then
						If (types And MetadataType.Field) > 0 Then
							item.AddField("OrderId")
							item.AddField("CustomerId")
						End If
					ElseIf item.Name = "Order Details" Then
						If (types And MetadataType.Field) > 0 Then
							item.AddField("OrderId")
							item.AddField("ProductId")
						End If

						If (types And MetadataType.ForeignKey) > 0 Then
							Dim foreignKey As MetadataForeignKey = item.AddForeignKey("OrderDetailsToOrder")
							foreignKey.Fields.Add("OrderId")
							foreignKey.ReferencedFields.Add("OrderId")
							Using name As New MetadataQualifiedName()
								name.Add("Orders")
								name.Add("dbo")

								foreignKey.ReferencedObjectName = name
							End Using
						End If
					End If
					Exit Select
			End Select

			item.Items.SetLoaded(types, True)
		End Sub

		'''///////////////////////////////////////////////////////////////////////
		''' 3rd way:
		'''
		''' This method demonstrates loading of metadata through .NET data providers 
		''' unsupported by our QueryBuilder component. If such data provider is able 
		''' to execute SQL queries, you can use our EventMetadataProvider with handling 
		''' it's ExecSQL event. In this event the EventMetadataProvider will provide 
		''' you SQL queries it use for the metadata retrieval. You have to execute 
		''' a query and return resulting data reader object.
		'''
		''' Note: In this sample we are using SQLiteSyntaxProvider. You have to use specific syntax providers in your application, 
		''' e.g. MySQLSyntaxProver, OracleSyntaxProvider, etc.
		'''///////////////////////////////////////////////////////////////////////
		Public Sub Way3()
			Dim queryBuilder1 = QueryBuilderStore.[Get]("LoadMetadata")

			Try
				_conn.Close()
				_conn.Open()

				' allow QueryBuilder to request metadata
				queryBuilder1.MetadataLoadingOptions.OfflineMode = False

				ResetQueryBuilderMetadata(queryBuilder1)
				queryBuilder1.SyntaxProvider = New SQLiteSyntaxProvider()

				queryBuilder1.MetadataProvider = New EventMetadataProvider()

				AddHandler DirectCast(queryBuilder1.MetadataProvider, EventMetadataProvider).ExecSQL, AddressOf way3EventMetadataProvider_ExecSQL
			Catch ex As Exception
				queryBuilder1.Message.[Error](ex.Message)
			End Try
		End Sub

		Private Sub way3EventMetadataProvider_ExecSQL(metadataProvider As BaseMetadataProvider, sql As String, schemaOnly As Boolean, ByRef dataReader As IDataReader)
			dataReader = Nothing

			If _conn IsNot Nothing Then
				Dim command As IDbCommand = _conn.CreateCommand()
				command.CommandText = sql
				dataReader = command.ExecuteReader()
			End If
		End Sub

		'''///////////////////////////////////////////////////////////////////////
		''' 4th way:
		''' This method demonstrates manual filling of metadata structure from 
		''' stored DataSet.
		'''///////////////////////////////////////////////////////////////////////
		Public Sub Way4()
			Dim queryBuilder1 = QueryBuilderStore.[Get]("LoadMetadata")
			ResetQueryBuilderMetadata(queryBuilder1)

			queryBuilder1.MetadataLoadingOptions.OfflineMode = True
			' prevent QueryBuilder to request metadata from connection
			Dim dataSet As New DataSet()

			' Load sample dataset created in the Visual Studio with Dataset Designer
			' and exported to XML using WriteXmlSchema() method.
			Dim xml = Path.Combine(Server.MapPath("~"), "../Sample databases/StoredDataSetSchema.xml")
			dataSet.ReadXmlSchema(xml)

			queryBuilder1.MetadataContainer.BeginUpdate()

			Try
				queryBuilder1.ClearMetadata()

				' add tables
				For Each table As DataTable In dataSet.Tables
					' add new metadata table
					Dim metadataTable As MetadataObject = queryBuilder1.MetadataContainer.AddTable(table.TableName)

					' add metadata fields (columns)
					For Each column As DataColumn In table.Columns
						' create new field
						Dim metadataField As MetadataField = metadataTable.AddField(column.ColumnName)
						' setup field
						metadataField.FieldType = TypeToDbType(column.DataType)
						metadataField.Nullable = column.AllowDBNull
						metadataField.[ReadOnly] = column.[ReadOnly]

						If column.MaxLength <> -1 Then
							metadataField.Size = column.MaxLength
						End If

						' detect the field is primary key
						For Each pkColumn As DataColumn In table.PrimaryKey
							If column Is pkColumn Then
								metadataField.PrimaryKey = True
							End If
						Next
					Next

					' add relations
					For Each relation As DataRelation In table.ParentRelations
						' create new relation on the parent table
						Dim metadataRelation As MetadataForeignKey = metadataTable.AddForeignKey(relation.RelationName)

						' set referenced table
						Using referencedName As New MetadataQualifiedName()
							referencedName.Add(relation.ParentTable.TableName)

							metadataRelation.ReferencedObjectName = referencedName
						End Using

						' set referenced fields
						For Each parentColumn As DataColumn In relation.ParentColumns
							metadataRelation.ReferencedFields.Add(parentColumn.ColumnName)
						Next

						' set fields
						For Each childColumn As DataColumn In relation.ChildColumns
							metadataRelation.Fields.Add(childColumn.ColumnName)
						Next
					Next
				Next
			Finally
				queryBuilder1.MetadataContainer.EndUpdate()
			End Try
		End Sub

		Private Shared Function TypeToDbType(type As Type) As DbType
			If type Is GetType(String) Then
				Return DbType.[String]
			End If
			If type Is GetType(Int16) Then
				Return DbType.Int16
			End If
			If type Is GetType(Int32) Then
				Return DbType.Int32
			End If
			If type Is GetType(Int64) Then
				Return DbType.Int64
			End If
			If type Is GetType(UInt16) Then
				Return DbType.UInt16
			End If
			If type Is GetType(UInt32) Then
				Return DbType.UInt32
			End If
			If type Is GetType(UInt64) Then
				Return DbType.UInt64
			End If
			If type Is GetType([Boolean]) Then
				Return DbType.[Boolean]
			End If
			If type Is GetType([Single]) Then
				Return DbType.[Single]
			End If
			If type Is GetType([Double]) Then
				Return DbType.[Double]
			End If
			If type Is GetType([Decimal]) Then
				Return DbType.[Decimal]
			End If
			If type Is GetType(DateTime) Then
				Return DbType.DateTime
			End If
			If type Is GetType(TimeSpan) Then
				Return DbType.Time
			End If
			If type Is GetType([Byte]) Then
				Return DbType.[Byte]
			End If
			If type Is GetType([SByte]) Then
				Return DbType.[SByte]
			End If
			If type Is GetType([Char]) Then
				Return DbType.[String]
			End If
			If type Is GetType([Byte]()) Then
				Return DbType.Binary
			End If
			If type Is GetType(Guid) Then
				Return DbType.Guid
			End If
			Return DbType.[Object]
		End Function

		Private Sub ResetQueryBuilderMetadata(queryBuilder1 As QueryBuilder)
			queryBuilder1.MetadataProvider = Nothing
			queryBuilder1.ClearMetadata()
			RemoveHandler queryBuilder1.MetadataContainer.ItemMetadataLoading, AddressOf way2ItemMetadataLoading
		End Sub
	End Class
End Namespace
