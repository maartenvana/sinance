using Sinance.Domain.Entities;

namespace Sinance.Business.Classes
{
    /// <summary>
    /// Import row for importing an transaction
    /// </summary>
    public class ImportRow
    {
        /// <summary>
        /// If this row already exists in the database
        /// </summary>
        public bool ExistsInDatabase { get; set; }

        /// <summary>
        /// If this row should actually get imported
        /// </summary>
        public bool Import { get; set; }

        /// <summary>
        /// Id of the import row
        /// </summary>
        public int ImportRowId { get; set; }

        /// <summary>
        /// The transaction that is going to be inserted
        /// </summary>
        public Transaction Transaction { get; set; }
    }
}