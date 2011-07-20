﻿using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using System.ComponentModel;
using System.Text;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing.Design;

[assembly: WebResource("HtmlEditorExtender.HtmlEditorExtenderBehavior.js", "text/javascript")]
[assembly: WebResource("HtmlEditorExtender.HtmlEditorExtenderBehavior.debug.js", "text/javascript")]
[assembly: WebResource("HtmlEditorExtender.HtmlEditorExtender_resource.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("HtmlEditorExtender.Images.html-editor-buttons.png", "img/png")]

namespace AjaxControlToolkit
{
    /// <summary>
    /// HtmlEditorExtender extends to a textbox and creates and renders an editable div 
    /// in place of targeted textbox.
    /// </summary>
    [TargetControlType(typeof(TextBox))]
    [RequiredScript(typeof(CommonToolkitScripts), 0)]
    [RequiredScript(typeof(ColorPickerExtender), 1)]
    [ClientScriptResource("Sys.Extended.UI.HtmlEditorExtenderBehavior", "HtmlEditorExtender.HtmlEditorExtenderBehavior.js")]
    [ClientCssResource("HtmlEditorExtender.HtmlEditorExtender_resource.css")]
    [ParseChildren(true)]
    [PersistChildren(false)]
    [System.Drawing.ToolboxBitmap(typeof(HtmlEditorExtender), "HtmlEditorExtender.html_editor_extender.ico")]
    //[Designer(typeof(HtmlEditorExtenderDesigner))]
    public class HtmlEditorExtender : ExtenderControlBase
    {
        internal const int ButtonWidthDef = 23;
        internal const int ButtonHeightDef = 21;
        HtmlEditorExtenderButtonCollection buttonList = null;

        public HtmlEditorExtender()
        {
            EnableClientState = true;
        }

        /// <summary>
        /// Provide button list to client side. Need help from Toolbar property 
        /// for designer experience support, cause Editor always blocks the property
        /// ability to provide values to client side as ExtenderControlProperty on run time.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]                
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [ExtenderControlProperty(true, true)]
        public HtmlEditorExtenderButtonCollection ToolbarButtons
        {
            get 
            {
                if (buttonList == null || buttonList.Count == 0)
                    CreateButtons();
                return buttonList;
            }
        }

        /// <summary>
        /// Helper property to cacth buttons from modifed buttons on design time.
        /// This property will only attached when Toolbar property are not empty in design time.
        /// </summary>
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(null)]
        [NotifyParentProperty(true)]
        [Editor(typeof(HtmlEditorExtenderButtonCollectionEditor), typeof(UITypeEditor))]
        [Description("Costumize visible buttons, leave empty to show all buttons")]
        public HtmlEditorExtenderButtonCollection Toolbar
        {
            get
            {
                if (buttonList == null || buttonList.Count == 0)
                    buttonList = new HtmlEditorExtenderButtonCollection();
                return buttonList;
            }
        }

        private string DecodeValues(string value)
        {
            if (buttonList == null || buttonList.Count == 0)
            {
                CreateButtons();
            }
            foreach (HtmlEditorExtenderButton button in buttonList)
            {
                value = button.Decode(value);
            }
            value = Decode(value);
            return value;
        }

        /// <summary>
        /// Decodes html tags those are not generated by any htmlEditorExtender button
        /// </summary>
        /// <param name="value">Value that contains html tags to decode</param>
        /// <returns>value after decoded</returns>
        protected virtual string Decode(string value)
        {
            //todo: cleanup style tagss no positioning
            string tags = "font|div|span|br|strong|em|strike|sub|sup|center|blockquote|hr|ol|ul|li|br|s|p|b|i|u";
            string attributes = "style|size|color|face|align";
            string attributeCharacters = "\\'\\(\\)\\,\\w\\-#\\s\\:\\;";
            var result = Regex.Replace(value, "\\&quot\\;", "\"", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, "(?:\\&lt\\;|\\<)(\\/?)((?:" + tags + ")(?:\\s(?:" + attributes + ")=\"[" + attributeCharacters + "]*\")*)(?:\\&gt\\;|\\>)", "<$1$2>", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
            string hrefCharacters = "^\\\"\\>\\<\\\\";
            result = Regex.Replace(result, "(?:\\&lt\\;|\\<)(\\/?)(a(?:(?:\\shref\\=\\\"[" + hrefCharacters + "]*\\\")|(?:\\sstyle\\=\\\"[" + attributeCharacters + "]*\\\"))*)(?:\\&gt\\;|\\>)", "<$1$2>", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
            result = Regex.Replace(result, "&amp;", "&", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, "&quot;", "\"", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, "&apos;", "'", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, "&nbsp;", "\xA0", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, "<[^>]*expression[^>]*>", "", RegexOptions.IgnoreCase | RegexOptions.ECMAScript);

            return result;
        }

        ///// <summary>
        ///// Get or Set width of button icon
        ///// </summary>
        //[ExtenderProvidedProperty]
        //[DefaultValue(ButtonWidthDef)]
        //public int ButtonWidth
        //{
        //    get { return GetPropertyValue<int>("ButtonWidth", 23); }
        //    set { SetPropertyValue<int>("ButtonWidth", value); }
        //}

        ///// <summary>
        ///// Get or set height of button icon
        ///// </summary>
        //[ExtenderProvidedProperty]
        //[DefaultValue(ButtonHeightDef)]
        //public int ButtonHeight
        //{
        //    get { return GetPropertyValue<int>("ButtonHeight", 21); }
        //    set { SetPropertyValue<int>("ButtonHeight", value); }
        //}

        /// <summary>
        /// On load method decode contents of textbox before render these to client side.
        /// </summary>
        /// <param name="e">event arguments</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Register an empty OnSubmit statement so the ASP.NET WebForm_OnSubmit method will be automatically
            // created and our behavior will be able to wrap it to encode html tags prior to submission
            ScriptManager.RegisterOnSubmitStatement(this, typeof(HtmlEditorExtender), "HtmlEditorExtenderOnSubmit", "null;");

            // If this extender has default focus, use ClientState to let it know
            ClientState = (string.Compare(Page.Form.DefaultFocus, TargetControlID, StringComparison.OrdinalIgnoreCase) == 0) ? "Focused" : null;

            // decode values of textbox
            TextBox txtBox = (TextBox)TargetControl;
            if (txtBox != null)
                txtBox.Text = DecodeValues(txtBox.Text);
        }

        /// <summary>
        /// When user defines/customize buttons on design time Toolbar property will accessed twice
        /// so we need to skip the first accessing of this property to avoid buttons created twice
        /// </summary>
        bool tracked = false;

        /// <summary>
        /// CreateButtons creates list of buttons for the toolbar
        /// </summary>        
        protected virtual void CreateButtons()
        {
            buttonList = new HtmlEditorExtenderButtonCollection();

            // avoid buttons for twice buttons craetion
            if (!tracked)
            {
                tracked = true;
                if (this.Site != null && this.Site.DesignMode)
                {
                return;
            }
            }
            tracked = false;
            buttonList.Add(new Undo());
            buttonList.Add(new Redo());
            buttonList.Add(new Bold());
            buttonList.Add(new Italic());
            buttonList.Add(new Underline());
            buttonList.Add(new StrikeThrough());
            buttonList.Add(new Subscript());
            buttonList.Add(new Superscript());
            buttonList.Add(new JustifyLeft());
            buttonList.Add(new JustifyCenter());
            buttonList.Add(new JustifyRight());
            buttonList.Add(new JustifyFull());
            buttonList.Add(new insertOrderedList());
            buttonList.Add(new insertUnorderedList());
            buttonList.Add(new CreateLink());
            buttonList.Add(new UnLink());
            //buttonList.Add(new FormatBlock());
            buttonList.Add(new RemoveFormat());
            //buttonList.Add(new InsertImage());
            buttonList.Add(new SelectAll());
            buttonList.Add(new UnSelect());
            buttonList.Add(new Delete());
            buttonList.Add(new Cut());
            buttonList.Add(new Copy());
            buttonList.Add(new Paste());
            buttonList.Add(new BackgroundColorSelector());
            buttonList.Add(new ForeColorSelector());
            buttonList.Add(new FontNameSelector());
            buttonList.Add(new FontSizeSelector());
            buttonList.Add(new Indent());
            buttonList.Add(new Outdent());
            buttonList.Add(new InsertHorizontalRule());
            buttonList.Add(new HorizontalSeparator());
        }
    }
}
