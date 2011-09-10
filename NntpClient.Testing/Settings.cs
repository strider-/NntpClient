using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Dynamic;

namespace NntpClient.Testing {
    public class Settings : DynamicObject {
        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            var s = ConfigurationManager.AppSettings;

            if(s[binder.Name] == null)
                result = null;
            else {
                var value = s[binder.Name];
                int x;

                if(int.TryParse(value, out x))
                    result = x;
                else
                    result = value;
            }

            return true;
        }
    }
}
