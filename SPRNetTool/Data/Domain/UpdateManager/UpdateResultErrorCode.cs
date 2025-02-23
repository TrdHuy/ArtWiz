using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtWiz.Data.Domain.UpdateManager
{
    public class UpdateResultErrorCode
    {
        public string Detail { get; private set; } = "";
        public string Name { get; private set; } = "";
        public int Code { get; private set; }

        public static UpdateResultErrorCode NONE = new UpdateResultErrorCode
        {
            Code = 0,
            Detail = "none",
            Name = "NONE",
        };
        public static UpdateResultErrorCode FAILED_TO_GET_VERSION_DATA_FROM_SERVER = new UpdateResultErrorCode
        {
            Code = 1,
            Detail = "Failed to get version data from server!",
            Name = "FAILED_TO_GET_VERSION_DATA_FROM_SERVER",
        };

        public static UpdateResultErrorCode BRANCH_NOT_FOUND = new UpdateResultErrorCode
        {
            Code = 2,
            Detail = "Branch not found!",
            Name = "BRANCH_NOT_FOUND",
        };
        public static UpdateResultErrorCode UNKNOWN_EXCEPTION = new UpdateResultErrorCode
        {
            Code = 3,
            Detail = "unknown exception!",
            Name = "UNKNOWN_EXCEPTION",
        };

        // Override Equals để so sánh dựa trên Code
        public override bool Equals(object? obj)
        {
            if (obj is UpdateResultErrorCode other)
            {
                return this.Code == other.Code;
            }
            return false;
        }

        // Override GetHashCode để nhất quán với Equals
        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        // Toán tử ==
        public static bool operator ==(UpdateResultErrorCode left, UpdateResultErrorCode right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left is null || right is null)
                return false;

            return left.Code == right.Code;
        }

        // Toán tử !=
        public static bool operator !=(UpdateResultErrorCode left, UpdateResultErrorCode right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
