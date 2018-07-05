Imports System
Imports System.Web.UI
Imports ActiveQueryBuilder.Web.Server

namespace Samples
    Public Class BasePage
        Inherits Page
        ' //CUT:PRO{{
        Public Sub New()
            AddHandler PreLoad, AddressOf OnPreLoad
        End Sub

        Protected Sub OnPreLoad(sender As Object, eventArgs As EventArgs)
            QueryBuilderStore.Remove()
            QueryTransformerStore.Remove()
        End Sub
        ' //}}CUT:PRO
    End Class
End Namespace