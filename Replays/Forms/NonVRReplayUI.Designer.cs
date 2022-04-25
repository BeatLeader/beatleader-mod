namespace BeatLeader.Replays.Forms
{
    partial class NonVRReplayUI
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.playButton = new System.Windows.Forms.Button();
            this.movementLerpStatus = new System.Windows.Forms.CheckBox();
            this.overrideCameraState = new System.Windows.Forms.CheckBox();
            this.closeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // playButton
            // 
            this.playButton.BackColor = System.Drawing.Color.Cyan;
            this.playButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.playButton.Location = new System.Drawing.Point(249, 186);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(75, 23);
            this.playButton.TabIndex = 0;
            this.playButton.Text = "Pause";
            this.playButton.UseVisualStyleBackColor = false;
            this.playButton.Click += new System.EventHandler(this.HandlePauseButtonClicked);
            // 
            // movementLerpStatus
            // 
            this.movementLerpStatus.Appearance = System.Windows.Forms.Appearance.Button;
            this.movementLerpStatus.AutoSize = true;
            this.movementLerpStatus.BackColor = System.Drawing.Color.Cyan;
            this.movementLerpStatus.Enabled = false;
            this.movementLerpStatus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.movementLerpStatus.Location = new System.Drawing.Point(205, 186);
            this.movementLerpStatus.Name = "movementLerpStatus";
            this.movementLerpStatus.Size = new System.Drawing.Size(38, 23);
            this.movementLerpStatus.TabIndex = 1;
            this.movementLerpStatus.Text = "Lerp";
            this.movementLerpStatus.UseVisualStyleBackColor = false;
            // 
            // overrideCameraState
            // 
            this.overrideCameraState.Appearance = System.Windows.Forms.Appearance.Button;
            this.overrideCameraState.AutoSize = true;
            this.overrideCameraState.BackColor = System.Drawing.Color.Cyan;
            this.overrideCameraState.Enabled = false;
            this.overrideCameraState.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.overrideCameraState.Location = new System.Drawing.Point(161, 186);
            this.overrideCameraState.Name = "overrideCameraState";
            this.overrideCameraState.Size = new System.Drawing.Size(38, 23);
            this.overrideCameraState.TabIndex = 2;
            this.overrideCameraState.Text = "Cam";
            this.overrideCameraState.UseVisualStyleBackColor = false;
            // 
            // closeButton
            // 
            this.closeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.Location = new System.Drawing.Point(304, 3);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(26, 13);
            this.closeButton.TabIndex = 5;
            this.closeButton.UseVisualStyleBackColor = false;
            this.closeButton.Click += new System.EventHandler(this.Close);
            // 
            // NonVRReplayUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(332, 218);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.overrideCameraState);
            this.Controls.Add(this.movementLerpStatus);
            this.Controls.Add(this.playButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.Name = "NonVRReplayUI";
            this.Opacity = 0.8D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Replay Controls";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.CheckBox movementLerpStatus;
        private System.Windows.Forms.CheckBox overrideCameraState;
        private System.Windows.Forms.Button closeButton;
    }
}