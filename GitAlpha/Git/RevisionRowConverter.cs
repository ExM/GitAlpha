using GitAlpha.Extensions;

namespace GitAlpha.Git;

public static class RevisionRowConverter
{
	public static IList<RevisionRow> ToRevisionRow(this IReadOnlyList<GitRevision> gitRevisions)
	{
		var result = gitRevisions.Select(revision => 
			new RevisionRow()
			{
				Id = revision.ObjectId,
				ParentIds = revision.ParentIds!,
				Author = revision.Author,
				Subject = revision.Subject,
				CommitDate = revision.CommitDate
			})
			.ToList();

		var currentColorId = 0;
		var colorMap = new Dictionary<ObjectId, int>();
		
		var first = result.FirstOrDefault();
		if (first is not null)
		{
			first.Graph.NodeIndex = 0;
			first.Render = new List<ObjectId>() { first.Id };
			first.Graph.ColorId = currentColorId;
			colorMap.Add(first.Id, currentColorId);
			currentColorId++;
		}

		foreach (var pair in result.Zip(result.Skip(1), (a, b) => new {a, b}))
		{
			var uRow = pair.a;
			var dRow = pair.b;

			if (uRow.ParentIds.Count == 1)
			{
				var parentId = uRow.ParentIds[0];
				if(!colorMap.ContainsKey(parentId))
					colorMap.Add(parentId, uRow.Graph.ColorId);
			}
			else
			{
				foreach (var parentId in uRow.ParentIds)
				{
					if (!colorMap.ContainsKey(parentId))
					{
						colorMap.Add(parentId, currentColorId);
						currentColorId++;
					}
				}
			}
			
			var uNodeIndex = uRow.Render.IndexOf(uRow.Id); // always exists
			var render = new List<ObjectId>(uRow.Render);
			render.RemoveAt(uNodeIndex);

			var missingParents = uRow.ParentIds.Where(parentId => !render.Contains(parentId)).ToList();
			if (missingParents.Remove(dRow.Id))
				render.InsertOrAdd(uNodeIndex, dRow.Id);
			foreach (var parentId in missingParents)
				render.InsertOrAdd(uNodeIndex, parentId);

			if (!render.Contains(dRow.Id))
			{
				var transitIndex = uRow.Render.IndexOf(dRow.Id);
				if (transitIndex != -1)
				{ // terminate transit if exists
					render.InsertOrAdd(transitIndex, dRow.Id);
				}
				else
				{ // new node
					render.Add(dRow.Id);
				}
			}

			dRow.Graph.NodeIndex =  render.IndexOf(dRow.Id);
			dRow.Render = render;
			if (colorMap.TryGetValue(dRow.Id, out var existColorId))
			{
				dRow.Graph.ColorId = existColorId;
			}
			else
			{
				dRow.Graph.ColorId = currentColorId;
				colorMap.Add(dRow.Id, currentColorId);
				currentColorId++;
			}
	
			var connect = new List<NodeConnection>();

			for(var uIdx = 0; uIdx < uRow.Render.Count; uIdx++)
			{
				var uId = uRow.Render[uIdx];
				if(uId == uRow.Id)
				{ // parent connections
					foreach (var pId in uRow.ParentIds)
					{
						var dIdx = dRow.Render.IndexOf(pId);
						if (dIdx != -1)
						{
							connect.Add(new NodeConnection(colorMap[pId], uIdx, dIdx));
						}
					}
				}
				else
				{ // transit connections
					var dIdx = dRow.Render.IndexOf(uId);
					if (dIdx != -1)
					{
						connect.Add(new NodeConnection(colorMap[uId], uIdx, dIdx));
					}
				}
			}

			foreach (var (colorId, uIdx, dIdx) in connect)
			{
				uRow.Graph.ConnectionsRender.Add(new RevisionGraphRow.Connections(){ Index = uIdx, Delta = dIdx - uIdx, ColorId = colorId, Up = false});
				dRow.Graph.ConnectionsRender.Add(new RevisionGraphRow.Connections(){ Index = dIdx, Delta = uIdx - dIdx, ColorId = colorId, Up = true});
			}
		}

		return result;
	}
	
	private record NodeConnection(int ColorId, int UpIndex, int DownIndex);
 }
