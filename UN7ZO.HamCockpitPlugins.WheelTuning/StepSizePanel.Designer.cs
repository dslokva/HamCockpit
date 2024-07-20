namespace UN7ZO.HamCockpitPlugins.WheelTuning
{
    partial class StepSizePanel
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
            this.stepSizeLabel = new System.Windows.Forms.Label();
            this.cboxStepSize = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // stepSizeLabel
            // 
            this.stepSizeLabel.BackColor = System.Drawing.SystemColors.Control;
            this.stepSizeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.stepSizeLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.stepSizeLabel.Location = new System.Drawing.Point(2, 8);
            this.stepSizeLabel.Margin = new System.Windows.Forms.Padding(0);
            this.stepSizeLabel.Name = "stepSizeLabel";
            this.stepSizeLabel.Size = new System.Drawing.Size(51, 22);
            this.stepSizeLabel.TabIndex = 4;
            this.stepSizeLabel.Text = " Step:";
            this.stepSizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.stepSizeLabel.Click += new System.EventHandler(this.stepSizeLabel_Click);
            // 
            // cboxStepSize
            // 
            this.cboxStepSize.FormattingEnabled = true;
            this.cboxStepSize.Location = new System.Drawing.Point(62, 6);
            this.cboxStepSize.Name = "cboxStepSize";
            this.cboxStepSize.Size = new System.Drawing.Size(75, 24);
            this.cboxStepSize.TabIndex = 3;
            this.cboxStepSize.SelectedIndexChanged += new System.EventHandler(this.cboxStepSize_SelectedIndexChanged);
            // 
            // StepSizePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.stepSizeLabel);
            this.Controls.Add(this.cboxStepSize);
            this.Name = "StepSizePanel";
            this.Size = new System.Drawing.Size(155, 34);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label stepSizeLabel;
        private System.Windows.Forms.ComboBox cboxStepSize;
    }
}
