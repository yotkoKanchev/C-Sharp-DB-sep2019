﻿namespace MusicHub.DataProcessor.ExportDtos
{
    public class ExportAlbumDto
    {
        public string AlbumName { get; set; }

        public string ReleaseDate { get; set; }

        public string ProducerName { get; set; }

        public ExportSongDto[] Songs { get; set; }

        public string AlbumPrice { get; set; }
    }
}
