import axiosClient from '../axiosClient';

// Function to fetch the image for a given articuloId
export const fetchImage = async (articuloId) => {
  try {
    console.log(articuloId)
    const response = await axiosClient.get(`api/Articulos/GetImage/${articuloId}`, {
      responseType: 'blob',
    });

    // Convert blob to a usable object URL
    const imageUrl = URL.createObjectURL(response.data);
    return imageUrl;
  } catch (error) {
    console.error(`Error fetching image for articuloId ${articuloId}:`, error);
    return null;
  }
};

export const fetchImagenes = async (imagenIds) => {
  try {
    // Fetch all image URLs in parallel
    const imageUrls = await Promise.all(
      imagenIds.map(async (imagenId) => {
        try {
          const response = await axiosClient.get(`api/Articulos/GetImagenGlobal/${imagenId}`, {
            responseType: 'blob',
          });

          // Convert blob to a usable object URL
          const imageUrl = URL.createObjectURL(response.data);
          return imageUrl;
        } catch (error) {
          console.error(`Error fetching image for imagenId ${imagenId}:`, error);
          return null; // Return null for failed fetches
        }
      })
    );

    // Filter out any null values (failed fetches)
    return imageUrls.filter((url) => url !== null);
  } catch (error) {
    console.error("Error fetching images:", error);
    return []; // Return an empty array in case of a general error
  }
};
export const getCharacteristicImage = async (characteristicId)=> {
  return axiosClient.get(`/api/Caracteristicas/GetImage/${characteristicId}`, {
    responseType: 'blob'
  });
}
export const fetchImagesAndIds = async (imagenIds) => {
  try {
    // Fetch all image URLs in parallel
    const imageResults = await Promise.all(
      imagenIds.map(async (imagenId) => {
        try {
          const response = await axiosClient.get(`api/Articulos/GetImagenGlobal/${imagenId}`, {
            responseType: 'blob',
          });

          // Convert blob to a usable object URL
          const imageUrl = URL.createObjectURL(response.data);
          return { id: imagenId, url: imageUrl }; // Return an object with id and url
        } catch (error) {
          console.error(`Error fetching image for imagenId ${imagenId}:`, error);
          return null; // Return null for failed fetches
        }
      })
    );

    // Filter out any null values (failed fetches)
    return imageResults.filter((result) => result !== null);
  } catch (error) {
    console.error("Error fetching images:", error);
    return []; // Return an empty array in case of a general error
  }
};
// Function to fetch articles with images and stock > 0
export const fetchArticulos = async () => {
  try {
    const response = await axiosClient.get("/GetInventarioGeneral?InstitucionID=1");
    if (response.data && response.data.data) {
      const validItems = response.data.data.filter(item => item.cantidad > 0);
      return await Promise.all(
        validItems.map(async (item) => {
          const imageUrl = await fetchImage(item.articulo.articuloId);
          return {
            ...item.articulo,
            cantidad: item.cantidad,
            imageUrl,
          };
        })
      );
    } else {
      console.error("Invalid API response:", response.data);
      return [];
    }
  } catch (error) {
    console.error("Error fetching articles:", error);
    return [];
  }
};
