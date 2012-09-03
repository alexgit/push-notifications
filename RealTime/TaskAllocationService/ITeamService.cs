using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskAllocationService
{
    public interface ITeamService
    {
        int[] GetUsersForTeam(int teamId);
    }

    public class TeamServiceImpl : ITeamService
    {
        private IDictionary<int, int[]> teams = new Dictionary<int, int[]> 
        {
            { 1, new [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } },
            { 2, new [] { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 } },
            { 3, new [] { 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 } }
        };

        public int[] GetUsersForTeam(int teamId)
        {
            return teams[teamId];
        }
    }
}
