using System;
using System.Collections.Generic;
using System.Text;
using PhotoProcessor.DTO.enums;

namespace PhotoProcessor.DTO.ServiceDTOs
{
    public sealed record JobMessage(
      Guid JobId,
      Guid MediaId,
      JobTypes JobType,
      string MediaUri,
      double? TimestampMs = null);

}
