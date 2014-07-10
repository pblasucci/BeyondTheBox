<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Main
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
    Me.StockLabel = New System.Windows.Forms.Label()
    Me.StockComboBox = New System.Windows.Forms.ComboBox()
    Me.SuspendLayout
    '
    'StockLabel
    '
    Me.StockLabel.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom)  _
            Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.StockLabel.BackColor = System.Drawing.Color.White
    Me.StockLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
    Me.StockLabel.Font = New System.Drawing.Font("Consolas", 32!, System.Drawing.FontStyle.Bold)
    Me.StockLabel.ForeColor = System.Drawing.Color.White
    Me.StockLabel.Location = New System.Drawing.Point(12, 107)
    Me.StockLabel.Margin = New System.Windows.Forms.Padding(3, 10, 3, 10)
    Me.StockLabel.Name = "StockLabel"
    Me.StockLabel.Size = New System.Drawing.Size(434, 98)
    Me.StockLabel.TabIndex = 0
    Me.StockLabel.Text = "0.0"
    Me.StockLabel.TextAlign = System.Drawing.ContentAlignment.BottomRight
    Me.StockLabel.UseMnemonic = false
    '
    'StockComboBox
    '
    Me.StockComboBox.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left)  _
            Or System.Windows.Forms.AnchorStyles.Right),System.Windows.Forms.AnchorStyles)
    Me.StockComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend
    Me.StockComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems
    Me.StockComboBox.Font = New System.Drawing.Font("Consolas", 28!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0,Byte))
    Me.StockComboBox.Items.AddRange(New Object() {"AAPL", "BBRY", "GOOG", "MSFT", "YHOO"})
    Me.StockComboBox.Location = New System.Drawing.Point(12, 12)
    Me.StockComboBox.MaxDropDownItems = 5
    Me.StockComboBox.Name = "StockComboBox"
    Me.StockComboBox.Size = New System.Drawing.Size(434, 74)
    Me.StockComboBox.Sorted = true
    Me.StockComboBox.TabIndex = 1
    '
    'Main
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(9!, 20!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.BackColor = System.Drawing.Color.Linen
    Me.ClientSize = New System.Drawing.Size(458, 224)
    Me.Controls.Add(Me.StockComboBox)
    Me.Controls.Add(Me.StockLabel)
    Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
    Me.MaximizeBox = false
    Me.MinimizeBox = false
    Me.Name = "Main"
    Me.Text = "tickz"
    Me.ResumeLayout(false)

End Sub
    Friend WithEvents StockLabel As System.Windows.Forms.Label
    Friend WithEvents StockComboBox As System.Windows.Forms.ComboBox
End Class
