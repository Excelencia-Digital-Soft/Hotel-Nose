import axiosClient from '../axiosClient';

// Function to fetch the image for a given articuloId
export const fetchImage = async (articuloId) => {
  try {
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

// Function to fetch articles with images and stock > 0
export const fetchArticulos = async () => {
  try {
    const response = await axiosClient.get("/GetInventarioGeneral");
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
