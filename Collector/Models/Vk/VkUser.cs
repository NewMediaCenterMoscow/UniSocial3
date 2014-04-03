using Collector.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collector.Models.Vk
{
	public enum VkUserSex
	{
		Undefined,
		Female,
		Male
	}
	public class VkUser
	{
		[JsonProperty("id")]
		public long Id { get; set; }

		[JsonProperty("first_name")]
		public string FirstName { get; set; }

		[JsonProperty("last_name")]
		public string LastName { get; set; }

		[JsonProperty("sex")]
		public VkUserSex Sex { get; set; }

		[JsonProperty("nickname")]
		public string Nickname { get; set; }

		[JsonProperty("screen_name")]
		public string ScreenName { get; set; }

		[JsonProperty("bdate")]
		public string BDate { get; set; }

		[JsonProperty("country")]
		public VkCityCountry Country { get; set; }

		[JsonProperty("city")]
		public VkCityCountry City { get; set; }

		[JsonProperty("deactivated")]
		[JsonConverter(typeof(VkUserDeactivatedConverted))]
		public bool Deactivated { get; set; }

		[JsonProperty("timezone")]
		public int Timezone { get; set; }

		[JsonProperty("photo_50")]
		public string Photo50 { get; set; }

		[JsonProperty("photo_100")]
		public string Photo100 { get; set; }

		[JsonProperty("photo_200")]
		public string Photo200 { get; set; }

		[JsonProperty("photo_max_orig")]
		public string PhotoMaxOrig { get; set; }

		[JsonProperty("has_mobile")]
		public bool HasMobile { get; set; }

		[JsonProperty("online")]
		public bool Online { get; set; }

		[JsonProperty("mobile_phone")]
		public string MobilePhone { get; set; }

		[JsonProperty("home_phone")]
		public string HomePhone { get; set; }

		[JsonProperty("university")]
		public long University { get; set; }

		[JsonProperty("university_name")]
		public string UniversityName { get; set; }

		[JsonProperty("faculty")]
		public long Faculty { get; set; }

		[JsonProperty("faculty_name")]
		public string FacultyName { get; set; }

		[JsonProperty("graduation")]
		public int Graduation { get; set; }
	}
}
