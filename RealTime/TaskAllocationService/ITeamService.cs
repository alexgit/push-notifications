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
}
