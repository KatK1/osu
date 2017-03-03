﻿using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Screens.Tournament
{
    public class GroupsContainer : Container
    {
        private FlowContainer<Group> topGroups;
        private FlowContainer<Group> bottomGroups;

        private List<Group> allGroups = new List<Group>();

        private int maxTeams;
        private int currentGroup;

        public GroupsContainer(int numGroups, int teamsPerGroup)
        {
            maxTeams = teamsPerGroup;

            char nextGroupName = 'A';

            Children = new[]
            {
                topGroups = new FillFlowContainer<Group>()
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,

                    AutoSizeAxes = Axes.Both,

                    Spacing = new Vector2(7f, 0)
                },
                bottomGroups = new FillFlowContainer<Group>()
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,

                    AutoSizeAxes = Axes.Both,

                    Spacing = new Vector2(7f, 0)
                }
            };

            for (int i = 0; i < numGroups; i++)
            {
                Group g = new Group(nextGroupName.ToString());

                allGroups.Add(g);
                nextGroupName++;

                if (i < (int)Math.Ceiling(numGroups / 2f))
                    topGroups.Add(g);
                else
                    bottomGroups.Add(g);
            }
        }

        public void AddTeam(Team team)
        {
            if (allGroups[currentGroup].TeamsCount == maxTeams)
                return;

            allGroups[currentGroup].AddTeam(team);

            currentGroup = (currentGroup + 1) % allGroups.Count;
        }

        public bool ContainsTeam(string fullName)
        {
            return allGroups.Any(g => g.ContainsTeam(fullName));
        }

        public void ClearTeams()
        {
            foreach (Group g in allGroups)
                g.ClearTeams();

            currentGroup = 0;
        }

        public string ToStringRepresentation()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Group g in allGroups)
            {
                if (g != allGroups.First())
                    sb.AppendLine();
                sb.AppendLine($"Group {g.GroupName}");
                sb.Append(g.ToStringRepresentation());
            }

            return sb.ToString();
        }
    }
}
