using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RigoFunc.IdentityServer.IntergrationTests.Comparers {
    public class ClaimComparer : IEqualityComparer<Claim> {
        public bool Equals(Claim x, Claim y) {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.Subject == y.Subject
                   && x.Issuer == y.Issuer
                   && x.OriginalIssuer == y.OriginalIssuer
                   && x.Type == y.Type
                   && x.Value == y.Value
                   && x.ValueType == y.ValueType
                   && x.Subject.Equals(y.Subject)
                   && x.Properties.SequenceEqual(y.Properties)
                   ;
        }

        public int GetHashCode(Claim obj) {
            //Check whether the object is null
            if (Object.ReferenceEquals(obj, null)) return 0;

            //Get hash code for the Name field if it is not null.

            //Get hash code for the Code field.

            //Calculate the hash code for the product.
            return
                (obj.Issuer?.GetHashCode() ?? 0)
                ^ (obj.OriginalIssuer?.GetHashCode() ?? 0)
                ^ (obj.Type?.GetHashCode() ?? 0)
                ^ (obj.Value?.GetHashCode() ?? 0)
                ^ (obj.ValueType?.GetHashCode() ?? 0)
                ^ (obj.Subject?.GetHashCode() ?? 0)
                ^ (obj.Properties?.GetHashCode() ?? 0)
                ;
        }
    }
}
