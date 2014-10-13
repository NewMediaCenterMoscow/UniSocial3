using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Model;
using Common.Database.Writers;

namespace Common.Database
{
	public class ObjectWritersFactory
	{
		private System.Data.SqlClient.SqlConnection sqlConn;

		public ObjectWritersFactory(System.Data.SqlClient.SqlConnection sqlConn)
		{
			// TODO: Complete member initialization
			this.sqlConn = sqlConn;
		}

		public IObjectWriter CreateWriter(CollectTask task)
		{
			if (task.SocialNetwork == SocialNetwork.VKontakte)
			{
				switch (task.Method)
				{
					case "groups.getById":
						return new VkGroupsDbObjectWriter(this.sqlConn);
					case "wall.getComments":
						return new VkCommentsDbObjectWriter(this.sqlConn);
					case "friends.get":
						return new VkFriendsDbObjectWriter(this.sqlConn);
					case "likes.getList":
						return new VkLikesDbObjectWriter(this.sqlConn);
					case "users.get":
						return new VkUsersDbObjectWriter(this.sqlConn);
					case "wall.get":
					case "wall.getReposts":
						return new VkPostsDbObjectWriter(this.sqlConn);
					case "groups.getMembers":
					case "groups.get":
					case "users.getSubscriptions":
						return new VkUserGroupsDbObjectWriter(this.sqlConn);
					default:
						throw new NotSupportedException(task.ToString());
				}
			}

			throw new NotSupportedException(task.ToString());
		}
	}
}
