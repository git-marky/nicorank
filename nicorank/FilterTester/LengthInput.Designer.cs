namespace nicorank.FilterTester
{
    partial class LengthInput
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labelLengthLegend = new System.Windows.Forms.Label();
            this.textBoxSeconds = new System.Windows.Forms.TextBox();
            this.textBoxMinutes = new System.Windows.Forms.TextBox();
            this.labelMinutesLegend = new System.Windows.Forms.Label();
            this.labelSecondsLegendLabel = new System.Windows.Forms.Label();
            this.textErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.textErrorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // labelLengthLegend
            // 
            this.labelLengthLegend.AutoSize = true;
            this.labelLengthLegend.Location = new System.Drawing.Point(3, 6);
            this.labelLengthLegend.Name = "labelLengthLegend";
            this.labelLengthLegend.Size = new System.Drawing.Size(31, 12);
            this.labelLengthLegend.TabIndex = 0;
            this.labelLengthLegend.Text = "長さ：";
            // 
            // textBoxSeconds
            // 
            this.textBoxSeconds.Location = new System.Drawing.Point(103, 3);
            this.textBoxSeconds.Name = "textBoxSeconds";
            this.textBoxSeconds.Size = new System.Drawing.Size(40, 19);
            this.textBoxSeconds.TabIndex = 3;
            this.textBoxSeconds.Text = "0";
            this.textBoxSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxSeconds.Validated += new System.EventHandler(this.textBox_Validated);
            this.textBoxSeconds.Validating += new System.ComponentModel.CancelEventHandler(this.secondsTextBox_Validating);
            // 
            // textBoxMinutes
            // 
            this.textBoxMinutes.Location = new System.Drawing.Point(34, 3);
            this.textBoxMinutes.Name = "textBoxMinutes";
            this.textBoxMinutes.Size = new System.Drawing.Size(40, 19);
            this.textBoxMinutes.TabIndex = 1;
            this.textBoxMinutes.Text = "0";
            this.textBoxMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxMinutes.Validated += new System.EventHandler(this.textBox_Validated);
            this.textBoxMinutes.Validating += new System.ComponentModel.CancelEventHandler(this.minutesTextBox_Validating);
            // 
            // labelMinutesLegend
            // 
            this.labelMinutesLegend.AutoSize = true;
            this.labelMinutesLegend.Location = new System.Drawing.Point(80, 6);
            this.labelMinutesLegend.Name = "labelMinutesLegend";
            this.labelMinutesLegend.Size = new System.Drawing.Size(17, 12);
            this.labelMinutesLegend.TabIndex = 2;
            this.labelMinutesLegend.Text = "分";
            // 
            // labelSecondsLegendLabel
            // 
            this.labelSecondsLegendLabel.AutoSize = true;
            this.labelSecondsLegendLabel.Location = new System.Drawing.Point(149, 6);
            this.labelSecondsLegendLabel.Name = "labelSecondsLegendLabel";
            this.labelSecondsLegendLabel.Size = new System.Drawing.Size(17, 12);
            this.labelSecondsLegendLabel.TabIndex = 4;
            this.labelSecondsLegendLabel.Text = "秒";
            // 
            // textErrorProvider
            // 
            this.textErrorProvider.ContainerControl = this;
            // 
            // LengthInput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelSecondsLegendLabel);
            this.Controls.Add(this.labelMinutesLegend);
            this.Controls.Add(this.textBoxMinutes);
            this.Controls.Add(this.textBoxSeconds);
            this.Controls.Add(this.labelLengthLegend);
            this.Name = "LengthInput";
            this.Size = new System.Drawing.Size(170, 25);
            ((System.ComponentModel.ISupportInitialize)(this.textErrorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelLengthLegend;
        private System.Windows.Forms.TextBox textBoxSeconds;
        private System.Windows.Forms.TextBox textBoxMinutes;
        private System.Windows.Forms.Label labelMinutesLegend;
        private System.Windows.Forms.Label labelSecondsLegendLabel;
        private System.Windows.Forms.ErrorProvider textErrorProvider;
    }
}
