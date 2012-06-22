﻿
using System.Collections.Generic;
namespace AjaxControlToolkit.Sanitizer {
    class AntiXssSanitizerProvider: SanitizerProvider {

  
        private string _applicationName;

 

        public override string ApplicationName {
            get {
                return _applicationName;
            }
            set {
                _applicationName = value;
            }

        }

        public override bool RequiresFullTrust {
            get {
                return true;
            }
        }

        public override string GetSafeHtmlFragment(string htmlFragment, Dictionary<string, string[]> elementWhiteList, Dictionary<string, string[]> attributeWhiteList)
        {
            return Microsoft.Security.Application.Sanitizer.GetSafeHtmlFragment(htmlFragment);
        }

    }
}
