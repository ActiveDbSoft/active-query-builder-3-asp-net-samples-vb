Imports System.Configuration
Imports System.IO
Imports System.Web.Mvc
Imports ActiveQueryBuilder.Core
Imports ActiveQueryBuilder.Web.Server

Namespace Controllers
	Public Class DoubleClientRenderingController
		Inherits Controller
		Public Function Index() As ActionResult
			CreateFirstQueryBuilder()
			CreateSecondQueryBuilder()

			Return View()
		End Function

		''' <summary>
		''' Creates and initializes the first instance of the QueryBuilder object if it doesn't exist. 
		''' </summary>
		Private Sub CreateFirstQueryBuilder()
			' Get an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.[Get]("FirstClient")

			If queryBuilder IsNot Nothing Then
				Return
			End If

			' Create an instance of the QueryBuilder object
			queryBuilder = QueryBuilderStore.Factory.MsSql("FirstClient")
            
			' Denies metadata loading requests from the metadata provider
			queryBuilder.MetadataLoadingOptions.OfflineMode = True

			' Load MetaData from XML document. File name is stored in the "Web.config" file in [/configuration/appSettings/NorthwindXmlMetaData] key
			Dim path__1 = ConfigurationManager.AppSettings("NorthwindXmlMetaData")
			Dim xml = Path.Combine(Server.MapPath("~"), path__1)

			queryBuilder.MetadataContainer.ImportFromXML(xml)
		End Sub

		''' <summary>
		''' Creates and initializes the second instance of the QueryBuilder object if it doesn't exist. 
		''' </summary>
		Private Sub CreateSecondQueryBuilder()
			' Get an instance of the QueryBuilder object
			Dim queryBuilder = QueryBuilderStore.[Get]("SecondClient")

			If queryBuilder IsNot Nothing Then
				Return
			End If

			' Create an instance of the QueryBuilder object
			queryBuilder = QueryBuilderStore.Factory.DB2("SecondClient")
            
			' Denies metadata loading requests from the metadata provider
			queryBuilder.MetadataLoadingOptions.OfflineMode = True

			' Load MetaData from XML document. File name is stored in the "Web.config" file in [/configuration/appSettings/db2_sample_with_alt_names] key
			Dim path__1 = ConfigurationManager.AppSettings("Db2XmlMetaData")
			Dim xml = Path.Combine(Server.MapPath("~"), path__1)

			queryBuilder.MetadataContainer.ImportFromXML(xml)
		End Sub
	End Class
End Namespace
