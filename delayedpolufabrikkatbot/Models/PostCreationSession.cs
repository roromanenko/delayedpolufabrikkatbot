using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace delayedpolufabrikkatbot.Models
{
	public class PostCreationSession
	{
        public PostCreationStep CurrentStep { get; set; }
        public bool WaitingForTitle => CurrentStep == PostCreationStep.WaitingForTitle;
		public bool WaitingForContent => CurrentStep == PostCreationStep.WaitingForContent;

		public ObjectId? PostId { get; set; }
    }

	public enum PostCreationStep
	{
		WaitingForTitle,
		WaitingForContent,
	}
}
