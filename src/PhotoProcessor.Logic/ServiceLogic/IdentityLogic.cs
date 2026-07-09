using PhotoProcessor.DTO.enums;
using PhotoProcessor.DTO.ServiceDTOs;
using PhotoProcessor.Logic.EntityLogic;
using PhotoProcessor.State.Data.Entities;
using PhotoProcessor.State.Data.Queries;

namespace PhotoProcessor.Logic.ServiceLogic
{
    public interface IIdentityLogic
    { 
        Task AssignForMedia(Guid mediaId, CancellationToken cancellationToken = default); 
        Task<List<IdentitySummary>> ListIdentities(CancellationToken cancellationToken = default); 
        Task<List<FingerprintSummary>> GetFacesForTag(Guid tagId, CancellationToken cancellationToken = default);
    }

    public class IdentityLogic(
        IFingerprintQueries fingerprintQueries,
        FingerprintLogic fingerprintLogic,
        TagLogic tagLogic,
        TagTypeLogic tagTypeLogic) : IIdentityLogic
    { 
        private const double FingerprintMatchThreshold = 0.5;

        public async Task AssignForMedia(Guid mediaId, CancellationToken cancellationToken = default)
        {
            List<Fingerprint> faces = await fingerprintLogic.GetFor(mediaId, f => f.MediaId, cancellationToken);
            if (faces.Count == 0) return;

            Guid personTagTypeId = await EnsurePersonTagType(cancellationToken);

            foreach (Fingerprint face in faces)
            {
                if (face.TagId is not null) 
                    continue; 

                List<FingerprintNeighbour> neighbours = await fingerprintQueries.NearestNeighbours(
                    face.Embedding,
                    count: 1,
                    excludeMediaId: mediaId,
                    assignedOnly: true,
                    cancellationToken: cancellationToken);

                FingerprintNeighbour? nearest = neighbours.FirstOrDefault();

                Guid tagId;
                if (nearest is not null && nearest.Distance <= FingerprintMatchThreshold)
                {
                    tagId = nearest.Fingerprint.TagId!.Value;
                }
                else
                {
                    Tag identity = new()
                    {
                        TagId = Guid.NewGuid(),
                        TagTypeId = personTagTypeId,
                        Label = string.Empty,  
                    };

                    await tagLogic.Upsert(identity, cancellationToken);

                    tagId = identity.TagId;
                }

                face.TagId = tagId;
                await fingerprintLogic.Upsert(face, cancellationToken);
            }
        }

        public async Task<List<IdentitySummary>> ListIdentities(CancellationToken cancellationToken = default)
        {
            Guid? personTagTypeId = await FindPersonTagType(cancellationToken);
            if (personTagTypeId is null) return [];

            List<Tag> identities = await tagLogic.GetFor(personTagTypeId.Value, t => t.TagTypeId, cancellationToken);
            Dictionary<Guid, int> counts = await fingerprintQueries.FaceCountsByTag(cancellationToken);

            List<IdentitySummary> summaries = new();

            foreach (Tag identity in identities)
            {
                IdentitySummary summary = new()
                {
                    TagId = identity.TagId,
                    Label = identity.Label,
                    FaceCount = counts.GetValueOrDefault(identity.TagId),
                };
                summaries.Add(summary);
            }
             
            return summaries.OrderByDescending(i => i.FaceCount).ToList();
        }

        public async Task<List<FingerprintSummary>> GetFacesForTag(Guid tagId, CancellationToken cancellationToken = default)
        {
            List<Fingerprint> faces = await fingerprintLogic.GetFor(tagId, f => f.TagId, cancellationToken);

            List<FingerprintSummary> summaries = new();

            foreach (Fingerprint face in faces)
            {
                FingerprintSummary summary = new()
                {
                    FingerprintId = face.FingerprintId,
                    MediaId = face.MediaId,
                    TagId = face.TagId,
                    DetectionScore = face.DetectionScore,
                    BoundingX = face.BoundingX,
                    BoundingY = face.BoundingY,
                    BoundingWidth = face.BoundingWidth,
                    BoundingHeight = face.BoundingHeight,
                }; 
                summaries.Add(summary); 
            }

            return summaries;
        }

        private async Task<Guid> EnsurePersonTagType(CancellationToken cancellationToken)
        {
            Guid? existing = await FindPersonTagType(cancellationToken);
            if (existing is not null) return existing.Value;

            TagType personType = new()
            {
                TagTypeId = Guid.NewGuid(),
                TagCategory = (int)TagCategory.Person,
            };

            await tagTypeLogic.Upsert(personType, cancellationToken);

            return personType.TagTypeId;
        }

        private async Task<Guid?> FindPersonTagType(CancellationToken cancellationToken)
        {
            List<TagType> existing = await tagTypeLogic.GetFor((int)TagCategory.Person, x => x.TagCategory, cancellationToken);
            return existing.Count > 0 ? existing[0].TagTypeId : null;
        }
    }
}
